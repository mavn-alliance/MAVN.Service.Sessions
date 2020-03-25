using Lykke.HttpClientGenerator;

namespace Lykke.Service.Sessions.Client
{
    /// <inheritdoc cref="ISessionsServiceClient" />
    public class SessionsServiceClient : ISessionsServiceClient
    {
        public ISessionsApi SessionsApi { get; }

        public SessionsServiceClient(IHttpClientGenerator httpClientGenerator)
        {
            SessionsApi = httpClientGenerator.Generate<ISessionsApi>();
        }
    }
}
