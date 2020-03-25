using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Sessions.Client
{
    public class SessionsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}