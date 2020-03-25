using Autofac;
using JetBrains.Annotations;
using Lykke.Service.Sessions.Core.Services;
using Lykke.Service.Sessions.Services;
using Lykke.Service.Sessions.Settings;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace Lykke.Service.Sessions.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly SessionsSettings _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue.SessionsService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // DO NOT UPDATE REDIS TO 2.* Right now it works worse as 1.2
            builder.Register(context =>
                {
                    var connectionMultiplexer = ConnectionMultiplexer.Connect(_settings.Redis.ConnString);
                    connectionMultiplexer.IncludeDetailInExceptions = false;
                    return connectionMultiplexer;
                }).As<IConnectionMultiplexer>().SingleInstance();

            builder.RegisterType<RedisSessionsClient>()
                .As<ISessionsClient>()
                .SingleInstance()
                .WithParameter("redisInstanceName", _settings.Redis.InstanceName)
                .WithParameter("sessionIdleTimeout", _settings.Redis.SessionIdleTimeout)
                .WithParameter("phoneKeyTtl", _settings.Redis.PhoneKeyTtl);
        }
    }
}