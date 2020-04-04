using System;
using JetBrains.Annotations;

namespace MAVN.Service.Sessions.Contracts
{
    /// <summary>
    /// Session end event
    /// </summary>
    [PublicAPI]
    public class SessionEndedEvent
    {
        /// <summary>Customer id</summary>
        public string CustomerId { get; set; }

        /// <summary>Session end timestamp</summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>Session token</summary>
        public string Token { get; set; }
    }
}
