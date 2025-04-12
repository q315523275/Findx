using System.Threading;
using System.Threading.Tasks;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ消息发送器
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    ///     发送
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="exchangeName"></param>
    /// <param name="exchangeType"></param>
    /// <param name="routingKey"></param>
    /// <param name="durable"></param>
    /// <param name="autoDelete"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task PublishAsync<T>(T obj, string exchangeName, string exchangeType, string routingKey, bool durable = false, bool autoDelete = false, CancellationToken cancellationToken = default);
}