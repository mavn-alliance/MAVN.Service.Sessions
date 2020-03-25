using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.Sessions.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        public SessionsSettings SessionsService { get; set; }
    }
}