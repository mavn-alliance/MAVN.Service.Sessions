using JetBrains.Annotations;

namespace Lykke.Service.Sessions.Settings
{
    [UsedImplicitly]
    public class SessionsSettings
    {
        public DbSettings Db { get; set; }

        public RedisSettings Redis { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}