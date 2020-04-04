using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.Sessions.Attributes;
using MAVN.Service.Sessions.Client;
using MAVN.Service.Sessions.Client.Models;
using MAVN.Service.Sessions.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.Sessions.Controllers
{
    [Route("api/sessions")]
    [TypeFilter(typeof(SensitiveDataExceptionFilter))]
    public class SessionsController : Controller, ISessionsApi
    {
        private readonly ISessionsClient _sessionClient;
        private readonly IMapper _mapper;

        public SessionsController(ISessionsClient sessionClient, IMapper mapper)
        {
            _sessionClient = sessionClient;
            _mapper = mapper;
        }

        [HttpGet("activeusercount")]
        public Task<long> GetActiveUsersCountAsync()
        {
            return _sessionClient.GetActiveUsersCount();
        }

        [HttpGet("activeClientIds")]
        public Task<IReadOnlyCollection<string>> GetActiveClientIdsAsync()
        {
            return _sessionClient.GetActiveClientIdsAsync();
        }

        /// <summary>
        /// Returns active sessions for client
        /// </summary>
        /// <param name="clientId">A client id to search session</param>
        /// <returns>A list of client sessions</returns>
        [HttpGet("{clientId}/session")]
        public async Task<IReadOnlyCollection<ClientSession>> GetActiveSessionsAsync(string clientId)
        {
            var sessions = await _sessionClient.GetActiveSessions(clientId);
            var result = sessions.Select(_mapper.Map<ClientSession>).ToArray();
            return result;
        }

        /// <summary>
        /// Returns session information
        /// </summary>
        /// <param name="sessionToken">An id of the session</param>
        /// <returns>Session information</returns>
        [HttpGet("{sessionToken}")]
        public async Task<ClientSession> GetSessionAsync([SensitiveData] string sessionToken)
        {
            var session = await _sessionClient.GetAsync(sessionToken);
            return _mapper.Map<ClientSession>(session);
        }

        [HttpPatch("{sessionToken}")]
        public Task ExtendSessionAsync(string sessionToken, [FromBody]ExtendSessionRequest updateSessionRequest)
        {
            return _sessionClient.ExtendSession(sessionToken, updateSessionRequest.Ttl);
        }

        [HttpPost("{sessionToken}")]
        public Task RefreshSessionAsync([SensitiveData] string sessionToken)
        {
            return _sessionClient.RefreshSessionAsync(sessionToken);
        }

        [HttpPost("setTag")]
        public Task SetTagAsync([FromBody]SetTagRequest request)
        {
            return _sessionClient.SetTag(request.SessionToken, request.Tag);
        }

        [HttpDelete("{sessionToken}")]
        public Task DeleteSessionIfExistsAsync([SensitiveData] string sessionToken)
        {
            return _sessionClient.DeleteSessionIfExistsAsync(sessionToken);
        }

        [HttpDelete("byclient/{clientId}")]
        public Task DeleteClientSessionsAsync([SensitiveData] string clientId)
        {
            return _sessionClient.DeleteClientSessionsAsync(clientId);
        }

        [HttpPost("authenticate/{clientId}")]
        public async Task<ClientSession> AuthenticateAsync(string clientId, [FromBody] CreateSessionRequest request)
        {
            var clientSession = await _sessionClient.Authenticate(
                clientId,
                request.ClientInfo,
                request.Ttl);

            return _mapper.Map<ClientSession>(clientSession);
        }
    }
}
