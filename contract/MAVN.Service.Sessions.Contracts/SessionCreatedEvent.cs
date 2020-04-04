using System;
using JetBrains.Annotations;

namespace MAVN.Service.Sessions.Contracts
{
    /// <summary>
    /// Represents a session creation event
    /// </summary>
    [PublicAPI]
    public class SessionCreatedEvent
    {
        /// <summary>
        /// Represents the id of the customer 
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// Represents the timestamp of the creation of the session
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}