using Lykke.HttpClientGenerator;

namespace MAVN.Service.Sessions.Client
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
