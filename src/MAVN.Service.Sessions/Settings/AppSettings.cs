using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace MAVN.Service.Sessions.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        public SessionsSettings SessionsService { get; set; }
    }
}