using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Sessions.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnString { get; set; }

        public string SessionCreatedExchangeName { get; set; }

        public string SessionEndedExchangeName { get; set; }
    }
}
