using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.RabbitMQ
{
    public class RabbitMqConsumerFactory : IRabbitMqConsumerFactory, IDisposable
    {
        protected IServiceScope ServiceScope { get; }

        public RabbitMqConsumerFactory(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScope = serviceScopeFactory.CreateScope();
        }

        public IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue, string connectionName = null)
        {
            var consumer = ServiceScope.ServiceProvider.GetRequiredService<RabbitMqConsumer>();
            consumer.Initialize(exchange, queue, connectionName);
            return consumer;
        }

        public void Dispose()
        {
            ServiceScope?.Dispose();
        }
    }
}
