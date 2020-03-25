using System;
using System.Threading.Tasks;

namespace Lykke.Service.Sessions.Core.Publishers
{
    public interface ISessionCreatedEventPublisher
    {
        Task PublishAsync(string customerId, DateTime timestamp);
    }
}