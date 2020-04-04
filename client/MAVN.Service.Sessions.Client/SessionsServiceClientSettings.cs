using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.Sessions.Client
{
    public class SessionsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}