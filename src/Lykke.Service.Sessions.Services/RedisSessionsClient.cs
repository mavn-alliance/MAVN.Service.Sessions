using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Common.Redis;
using Lykke.Service.Sessions.Core.Domain;
using Lykke.Service.Sessions.Core.Publishers;
using Lykke.Service.Sessions.Core.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Lykke.Service.Sessions.Services
{
    public class RedisSessionsClient : ISessionsClient
    {
        private const string TagKeyPattern = "{0}:tag:{1}:id:{2}";
        private const string TokenKeyPattern = "{0}:token:{1}";
        private const string ClientKeyPattern = "{0}:clientId:{1}";

        private readonly ISessionCreatedEventPublisher _sessionCreatedEventPublisher;
        private readonly ISessionEndedEventPublisher _sessionEndedEventPublisher;
        private readonly ILog _log;
        private readonly string _onlineSessionsKey;
        private readonly TimeSpan _sessionKeyTtl;
        private readonly TimeSpan _phoneKeyTtl;
        private readonly IDatabase _db;
        private readonly string _redisInstanceName;

        public RedisSessionsClient(
            IConnectionMultiplexer connectionMultiplexer,
            string redisInstanceName,
            TimeSpan sessionIdleTimeout,
            TimeSpan phoneKeyTtl,
            ILogFactory logFactory,
            ISessionCreatedEventPublisher sessionCreatedEventPublisher,
            ISessionEndedEventPublisher sessionEndedEventPublisher,
            int dbId = -1)
        {
            _sessionCreatedEventPublisher = sessionCreatedEventPublisher;
            _sessionEndedEventPublisher = sessionEndedEventPublisher;
            _log = logFactory.CreateLog(this);
            _db = connectionMultiplexer.GetDatabase(dbId);
            _redisInstanceName = redisInstanceName;
            _sessionKeyTtl = sessionIdleTimeout;
            _phoneKeyTtl = phoneKeyTtl;
            _onlineSessionsKey = $"{_redisInstanceName}:online-sessions";
        }

        public Task<long> GetActiveUsersCount()
        {
            return _db.SortedSetLengthAsync(_onlineSessionsKey, DateTime.UtcNow.Ticks, double.MaxValue);
        }

        public async Task<IReadOnlyCollection<IClientSession>> GetActiveSessions(string clientId)
        {
            var key = string.Format(ClientKeyPattern, _redisInstanceName, clientId);
            var lastKnownSessionTokens = await _db.SetMembersAsync(key);
            if (lastKnownSessionTokens == null || !lastKnownSessionTokens.Any())
                return Array.Empty<IClientSession>();

            var aliveSessions = new List<ServerSession>();

            foreach (var sessionToken in lastKnownSessionTokens)
            {
                var sessionKey = string.Format(TokenKeyPattern, _redisInstanceName, sessionToken);
                var keyValue = await _db.StringGetAsync(sessionKey);
                if (keyValue.HasValue)
                {
                    var session = JsonConvert.DeserializeObject<ServerSession>(keyValue);
                    aliveSessions.Add(session);
                }
            }

            var deadTokens = lastKnownSessionTokens
                .Select(v => v.ToString())
                .Except(aliveSessions.Select(s => s.SessionToken))
                .Select(v => (RedisValue)v)
                .ToArray();
            if (deadTokens.Any())
                await _db.SetRemoveAsync(key, deadTokens);

            return aliveSessions;
        }

        public async Task<IReadOnlyCollection<string>> GetActiveClientIdsAsync()
        {
            var activeSessionIDs = await _db.SortedSetRangeByScoreAsync(_onlineSessionsKey, DateTime.UtcNow.Ticks, double.MaxValue);
            var sessionsValues = await _db.StringGetMultiKeyAsync(
                activeSessionIDs
                    .Select(key => (RedisKey) string.Format(TokenKeyPattern, _redisInstanceName, key))
                    .ToArray());

            var ids = sessionsValues.Where(item => item.HasValue)
                .Select(json => JsonConvert.DeserializeObject<ServerSession>(json))
                .Where(session => !string.IsNullOrEmpty(session.ClientId))
                .Select(session => session.ClientId)
                .Distinct()
                .ToArray();
            return ids;
        }

        public async Task<IClientSession> GetAsync(string sessionToken)
        {
            var sessionKey = string.Format(TokenKeyPattern, _redisInstanceName, sessionToken);
            var phoneTagKey = string.Format(TagKeyPattern, _redisInstanceName, "phone", sessionToken);

            var keyValue = await _db.StringGetAsync(sessionKey);
            if (!keyValue.HasValue)
                return null;

            var session = JsonConvert.DeserializeObject<ServerSession>(keyValue);

            var ttl = session.Ttl == default ? _sessionKeyTtl : session.Ttl;
            var tagsTask = _db.StringGetAsync(phoneTagKey);
            var steps = new List<Task>
            {
                tagsTask,
                _db.KeyExpireAsync(sessionKey, ttl),
                _db.KeyExpireAsync(phoneTagKey, ttl),
                _db.SortedSetAddAsync(_onlineSessionsKey, sessionToken, (DateTime.UtcNow + ttl).Ticks),
            };

            await Task.WhenAll(steps);

            var currentTag = tagsTask.Result;
            session.Tags = currentTag.HasValue ? new string[] {currentTag} : Array.Empty<string>();

            return session;
        }

        public async Task<IClientSession> Authenticate(
            string clientId,
            string clientInfo,
            TimeSpan? ttl = null)
        {
            ttl = ttl ?? _sessionKeyTtl;
            var now = DateTime.UtcNow;
            var sessionToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var sessionKey = string.Format(TokenKeyPattern, _redisInstanceName, sessionToken);

            var session = new ServerSession
            {
                AuthId = Guid.NewGuid(),
                ClientId = clientId,
                SessionToken = sessionToken,
                ClientInfo = clientInfo,
                Registered = now,
                Ttl = ttl.Value
            };

            var sessionByClientIdKey = string.Format(ClientKeyPattern, _redisInstanceName, clientId);

            var setKeyTask = _db.StringSetAsync(sessionKey, session.ToJson(), _sessionKeyTtl);

            var steps = new List<Task>
            {
                setKeyTask,
                _db.SortedSetAddAsync(_onlineSessionsKey, sessionToken, (DateTime.UtcNow + ttl.Value).Ticks),
                _db.SetAddAsync(sessionByClientIdKey, sessionToken),
            };

            await Task.WhenAll(steps);

            if (!setKeyTask.Result)
                throw new InvalidOperationException("error during creation a token");

            await _sessionCreatedEventPublisher.PublishAsync(clientId, DateTime.UtcNow);

            _log.Info(nameof(Authenticate), "Session is created", new { clientId });

            return session;
        }

        public async Task SetTag(string sessionToken, string tag)
        {
            var tagKey = string.Format(TagKeyPattern, _redisInstanceName, tag, sessionToken);

            var setResult = await _db.StringSetAsync(tagKey, tag, tag == "phone" ? _phoneKeyTtl : _sessionKeyTtl);

            if (!setResult)
                throw new InvalidOperationException("error during creation a token tag");
        }

        public Task RefreshSessionAsync(string sessionToken)
        {
            return GetAsync(sessionToken);
        }

        public async Task DeleteSessionIfExistsAsync(string sessionToken)
        {
            var sessionKey = string.Format(TokenKeyPattern, _redisInstanceName, sessionToken);
            var keyValue = await _db.StringGetAsync(sessionKey);
            bool sessionExist = keyValue.HasValue;

            var tasks = new List<Task>();
            ServerSession session = null;
            if (sessionExist)
            {
                tasks.Add(_db.KeyDeleteAsync(sessionKey));
                session = JsonConvert.DeserializeObject<ServerSession>(keyValue);
                var sessionByClientIdKey = string.Format(ClientKeyPattern, _redisInstanceName, session.ClientId);
                tasks.Add(_db.SetRemoveAsync(sessionByClientIdKey, sessionToken));
            }
            tasks.Add(_db.SortedSetRemoveAsync(_onlineSessionsKey, sessionToken));

            await Task.WhenAll(tasks);

            if (sessionExist)
                await _sessionEndedEventPublisher.PublishAsync(session.ClientId, sessionToken, DateTime.UtcNow);
            else
                _log.Warning($"Session with token {sessionToken} was not found for deletion.");

            _log.Info("Session is deleted");
        }

        public async Task DeleteClientSessionsAsync(string clientId)
        {
            var clientSessions = await GetActiveSessions(clientId);
            foreach (var clientSession in clientSessions)
            {
                await DeleteSessionIfExistsAsync(clientSession.SessionToken);
            }
        }

        public async Task ExtendSession(string sessionToken, TimeSpan ttl)
        {
            var session = await GetAsync(sessionToken);
            var sessionKey = string.Format(TokenKeyPattern, _redisInstanceName, session.ClientId);

            await _db.KeyExpireAsync(sessionKey, ttl);

            _log.Info(nameof(ExtendSession), "Session is extended",  new { clientId = session.ClientId });
        }
    }
}
