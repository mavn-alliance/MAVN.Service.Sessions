using System;
using System.Threading.Tasks;

namespace Lykke.Service.Sessions.Core.Publishers
{
    public interface ISessionEndedEventPublisher
    {
        Task PublishAsync(string customerId, string token, DateTime timestamp);
    }
}