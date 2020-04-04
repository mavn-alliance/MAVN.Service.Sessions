using System;
using JetBrains.Annotations;

namespace MAVN.Service.Sessions.Client.Models
{
    [PublicAPI]
    public class CreateSessionRequest
    {
        public string ClientInfo { get; set; }
        public TimeSpan? Ttl { get; set; }
    }
}
