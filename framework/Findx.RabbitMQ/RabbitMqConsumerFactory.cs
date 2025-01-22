using System;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.RabbitMQ;

public class RabbitMqConsumerFactory : IRabbitMqConsumerFactory, IDisposable
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    public RabbitMqConsumerFactory(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScope = serviceScopeFactory.CreateScope();
    }

    protected IServiceScope ServiceScope { get; }

    public void Dispose()
    {
        ServiceScope?.Dispose();
    }

    public IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue, string connectionName = null)
    {
        var consumer = ServiceScope.ServiceProvider.GetRequiredService<RabbitMqConsumer>();
        consumer.Initialize(exchange, queue, connectionName);
        return consumer;
    }
}