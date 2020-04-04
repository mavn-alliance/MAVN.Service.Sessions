using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.Sessions.Contracts;
using MAVN.Service.Sessions.Core.Publishers;

namespace MAVN.Service.Sessions.Rabbit.Publishers
{
    public class SessionEndedEventPublisher : JsonRabbitPublisher<SessionEndedEvent>, ISessionEndedEventPublisher
    {
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        public SessionEndedEventPublisher(ILogFactory logFactory, string connectionString, string exchangeName)
            : base(logFactory, connectionString, exchangeName)
        {
        }

        public async Task PublishAsync(string customerId, string token, DateTime timestamp)
        {
            try
            {
                await _sync.WaitAsync();

                await PublishAsync(new SessionEndedEvent
                {
                    CustomerId = customerId,
                    Token = token,
                    TimeStamp = timestamp
                });
            }
            finally
            {
                _sync.Release();
            }
        }
    }
}