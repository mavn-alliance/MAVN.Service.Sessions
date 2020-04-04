using System;
using MAVN.Service.Sessions.Core.Services;

namespace MAVN.Service.Sessions.Core.Domain
{
    public class ServerSession : IClientSession
    {
        public Guid AuthId { get; set; }
        public string ClientId { get; set; }
        public string SessionToken { get; set; }
        public string ClientInfo { get; set; }
        public DateTime Registered { get; set; }
        public string[] Tags { get; set; }
        public TimeSpan Ttl { get; set; }
    }
}
