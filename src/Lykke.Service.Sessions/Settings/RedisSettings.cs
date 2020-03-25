using System;

namespace Lykke.Service.Sessions.Settings
{
    public class RedisSettings
    {
        public string ConnString { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan SessionIdleTimeout { get; set; }
        public TimeSpan PhoneKeyTtl { get; set; }
    }
}
