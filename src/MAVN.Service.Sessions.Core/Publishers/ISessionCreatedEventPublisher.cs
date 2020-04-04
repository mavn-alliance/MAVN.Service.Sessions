using System;
using System.Threading.Tasks;

namespace MAVN.Service.Sessions.Core.Publishers
{
    public interface ISessionCreatedEventPublisher
    {
        Task PublishAsync(string customerId, DateTime timestamp);
    }
}