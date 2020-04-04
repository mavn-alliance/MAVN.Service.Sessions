using System;

namespace MAVN.Service.Sessions.Core.Services
{
    public interface IClientSession
    {
        Guid AuthId { get; set; }
        string ClientId { get; }
        string SessionToken { get; }
        string ClientInfo { get; }
        DateTime Registered { get; }
        string[] Tags { get; set; }
        TimeSpan Ttl { get; set; }
    }
}
