using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.Sessions.Client.Models;
using Refit;

namespace MAVN.Service.Sessions.Client
{
    /// <summary>
    /// Sessions service API for sessions.
    /// </summary>
    [PublicAPI]
    public interface ISessionsApi
    {
        [Get("/api/sessions/activeusercount")]
        Task<long> GetActiveUsersCountAsync();

        [Get("/api/sessions/activeClientIds")]
        Task<IReadOnlyCollection<string>> GetActiveClientIdsAsync();

        /// <summary>
        /// Returns active sessions for client
        /// </summary>
        /// <param name="clientId">A client id to search session</param>
        /// <returns>A list of client sessions</returns>
        [Get("/api/sessions/{clientId}/session")]
        Task<IReadOnlyCollection<ClientSession>> GetActiveSessionsAsync(string clientId);

        [Get("/api/sessions/{sessionToken}")]
        Task<ClientSession> GetSessionAsync(string sessionToken);

        [Patch("/api/sessions/{sessionToken}")]
        Task ExtendSessionAsync(string sessionToken, ExtendSessionRequest ttl);

        [Post("/api/sessions/{sessionToken}")]
        Task RefreshSessionAsync(string sessionToken);

        [Post("/api/sessions/setTag")]
        Task SetTagAsync(SetTagRequest request);

        [Delete("/api/sessions/{sessionToken}")]
        Task DeleteSessionIfExistsAsync(string sessionToken);

        [Delete("/api/sessions/byclient/{clientId}")]
        Task DeleteClientSessionsAsync(string clientId);

        [Post("/api/sessions/authenticate/{clientId}")]
        Task<ClientSession> AuthenticateAsync(string clientId, CreateSessionRequest request);
    }
}