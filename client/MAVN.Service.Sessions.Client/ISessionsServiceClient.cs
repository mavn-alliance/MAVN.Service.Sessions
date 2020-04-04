using JetBrains.Annotations;

namespace MAVN.Service.Sessions.Client
{
    /// <summary>
    /// An interface of the sessions service.
    /// </summary>
    [PublicAPI]
    public interface ISessionsServiceClient
    {
        ISessionsApi SessionsApi { get; }
    }
}