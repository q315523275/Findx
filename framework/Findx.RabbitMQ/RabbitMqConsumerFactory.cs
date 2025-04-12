using System;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.RabbitMQ;

/// <summary>
///     RabbitMQ消费者创建工厂
/// </summary>
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

    /// <summary>
    ///     
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="queue"></param>
    /// <param name="connectionName"></param>
    /// <returns></returns>
    public IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue, string connectionName = null)
    {
        var consumer = ServiceScope.ServiceProvider.GetRequiredService<RabbitMqConsumer>();
        consumer.Initialize(exchange, queue, connectionName);
        return consumer;
    }
}