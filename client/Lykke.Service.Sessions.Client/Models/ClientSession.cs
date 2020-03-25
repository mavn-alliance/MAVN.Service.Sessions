using System;
using JetBrains.Annotations;

namespace Lykke.Service.Sessions.Client.Models
{
    [PublicAPI]
    public class ClientSession
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
