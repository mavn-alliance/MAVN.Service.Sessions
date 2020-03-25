using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Sessions.Core.Services
{
    public interface ISessionsClient
    {
        Task<long> GetActiveUsersCount();
        Task<IReadOnlyCollection<string>> GetActiveClientIdsAsync();
        Task<IClientSession> GetAsync(string sessionToken);
        Task<IClientSession> Authenticate(
            string clientId,
            string clientInfo,
            TimeSpan? ttl = null);
        Task SetTag(string sessionToken, string tag);
        Task DeleteSessionIfExistsAsync(string sessionToken);
        Task DeleteClientSessionsAsync(string clientId);
        Task RefreshSessionAsync(string sessionToken);

        Task ExtendSession(string sessionToken, TimeSpan ttl);
        Task<IReadOnlyCollection<IClientSession>> GetActiveSessions(string clientId);
    }
}
