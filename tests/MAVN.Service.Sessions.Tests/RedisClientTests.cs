using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Logs;
using MAVN.Service.Sessions.Core.Publishers;
using MAVN.Service.Sessions.Services;
using NSubstitute;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace MAVN.Service.Sessions.Tests
{
    public class RedisClientTests
    {
        private readonly ITestOutputHelper _output;
        private readonly ISessionCreatedEventPublisher _startedPublisher;
        private readonly ISessionEndedEventPublisher _endedPublisher;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private RedisSessionsClient _client;

        public RedisClientTests(ITestOutputHelper output)
        {
            _output = output;
            _startedPublisher = Substitute.For<ISessionCreatedEventPublisher>();
            _endedPublisher = Substitute.For<ISessionEndedEventPublisher>();
            _connectionMultiplexer = ConnectionMultiplexer.Connect("");
            _client = new RedisSessionsClient(
                _connectionMultiplexer,
                "Sessions",
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                EmptyLogFactory.Instance, 
                _startedPublisher,
                _endedPublisher,
                2);
        }

        [Fact(Skip = "Only for manual testing")]
        public void PerformanceTest()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 1; i++)
            {
                var t = Task.Run(RunRedisRequest);
                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Authenticate_Creates_Record()
        {
            var session = await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");
            var result = await _client.GetAsync(session.SessionToken);

            Assert.Equal(session.AuthId, result.AuthId);
            Assert.Equal(session.ClientId, result.ClientId);
            Assert.Equal(session.SessionToken, result.SessionToken);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task GetActiveSessions_Returns_Sessions()
        {
            var clientId = Guid.NewGuid().ToString();
            var session1 = await _client.Authenticate(clientId, "SomeInfo");
            var session2 = await _client.Authenticate(clientId, "SomeInfo");

            var sessions = await _client.GetActiveSessions(clientId);

            Assert.NotNull(sessions);
            Assert.True(sessions.Count == 2);

            var sessionIDs = sessions.Select(s => s.SessionToken).ToArray();

            Assert.Contains(session1.SessionToken, sessionIDs);
            Assert.Contains(session2.SessionToken, sessionIDs);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Should_Return_SessionCount()
        {
            await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");
            await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");

            var count = await _client.GetActiveUsersCount();

            Assert.True(count > 1);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Should_Return_UserIds()
        {
            var user1 = Guid.NewGuid().ToString();
            var user2 = Guid.NewGuid().ToString();
            await _client.Authenticate(user1, "SomeInfo");
            await _client.Authenticate(user2, "SomeInfo");

            var result = await _client.GetActiveClientIdsAsync();

            Assert.Contains(user1, result);
            Assert.Contains(user2, result);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Should_Not_Return_UserIds()
        {
            _client = new RedisSessionsClient(
                _connectionMultiplexer,
                "Sessions",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                EmptyLogFactory.Instance,
                _startedPublisher,
                _endedPublisher);
            var user1 = Guid.NewGuid().ToString();
            var user2 = Guid.NewGuid().ToString();
            await _client.Authenticate(user1, "SomeInfo");
            await _client.Authenticate(user2, "SomeInfo");

            await Task.Delay(TimeSpan.FromSeconds(2));

            var result = await _client.GetActiveClientIdsAsync();

            Assert.Empty(result);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Session_Should_Expire()
        {
            _client = new RedisSessionsClient(
                _connectionMultiplexer,
                "Sessions",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                EmptyLogFactory.Instance,
                _startedPublisher,
                _endedPublisher);
            var session = await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");

            await Task.Delay(TimeSpan.FromSeconds(2));
            var result = await _client.GetAsync(session.SessionToken);

            Assert.Null(result);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Session_Should_Be_Refreshed_WithRefresh()
        {
            _client = new RedisSessionsClient(
                _connectionMultiplexer,
                "Sessions",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                EmptyLogFactory.Instance,
                _startedPublisher,
                _endedPublisher);
            var session = await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");

            await Task.Delay(TimeSpan.FromSeconds(0.8));

            await _client.RefreshSessionAsync(session.SessionToken);

            await Task.Delay(TimeSpan.FromSeconds(0.8));

            var result = await _client.GetAsync(session.SessionToken);

            Assert.NotNull(result);
        }

        [Fact(Skip = "Only for manual testing")]
        public async Task Session_Should_Be_Refreshed_WithGet()
        {
            _client = new RedisSessionsClient(
                _connectionMultiplexer,
                "Sessions",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                EmptyLogFactory.Instance,
                _startedPublisher,
                _endedPublisher);
            var session = await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");

            await Task.Delay(TimeSpan.FromSeconds(0.8));

            await _client.GetAsync(session.SessionToken);

            await Task.Delay(TimeSpan.FromSeconds(0.8));

            var result = await _client.GetAsync(session.SessionToken);

            Assert.NotNull(result);
        }

        private async Task RunRedisRequest()
        {
            var sw = new Stopwatch();
            for (var i = 0; i < 10; i++)
            {
                sw.Restart();
                await _client.Authenticate(Guid.NewGuid().ToString(), "SomeInfo");
                await _client.GetActiveUsersCount().ConfigureAwait(false);
                _output.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {sw.Elapsed}");
            }
        }
    }
}
