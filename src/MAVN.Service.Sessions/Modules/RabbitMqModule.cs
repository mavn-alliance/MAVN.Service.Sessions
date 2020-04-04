using Autofac;
using Common;
using JetBrains.Annotations;
using MAVN.Service.Sessions.Core.Publishers;
using MAVN.Service.Sessions.Rabbit.Publishers;
using MAVN.Service.Sessions.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.Sessions.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.SessionsService.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SessionCreatedEventPublisher>()
                .As<ISessionCreatedEventPublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter("connectionString", _settings.RabbitMqConnString)
                .WithParameter("exchangeName", _settings.SessionCreatedExchangeName)
                .SingleInstance();

            builder.RegisterType<SessionEndedEventPublisher>()
                .As<ISessionEndedEventPublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter("connectionString", _settings.RabbitMqConnString)
                .WithParameter("exchangeName", _settings.SessionEndedExchangeName)
                .SingleInstance();
        }
    }
}