using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Findx.Tracing;
using Findx.Extensions;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ推送者
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConnectionPool _connectionPool;
    private readonly IRabbitMqSerializer _serializer;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="serializer"></param>
    /// <param name="correlationIdProvider"></param>
    public RabbitMqPublisher(IConnectionPool connectionPool, IRabbitMqSerializer serializer, ICorrelationIdProvider correlationIdProvider)
    {
        _connectionPool = connectionPool;
        _serializer = serializer;
        _correlationIdProvider = correlationIdProvider;
    }

    /// <summary>
    ///     推送消息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="exchangeName"></param>
    /// <param name="exchangeType"></param>
    /// <param name="routingKey"></param>
    /// <param name="durable"></param>
    /// <param name="autoDelete"></param>
    /// <param name="cancellationToken"></param>
    public async Task PublishAsync<T>(T obj, string exchangeName, string exchangeType, string routingKey, bool durable = false, bool autoDelete = false, CancellationToken cancellationToken = default)
    {
        var body = _serializer.Serialize(obj);

        await using var channel = await (await _connectionPool.GetAsync(cancellationToken: cancellationToken)).CreateChannelAsync(cancellationToken: cancellationToken);
        
        // 创建并配置交换器
        await channel.ExchangeDeclareAsync(exchangeName, exchangeType, durable: durable, autoDelete: autoDelete, cancellationToken: cancellationToken);
        // 创建队列属性
        var properties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };
        if (properties.CorrelationId.IsNullOrWhiteSpace())
        {
            properties.CorrelationId = _correlationIdProvider.Get();
        }
        // 发送数据
        // channel.BasicPublish(exchangeName, routingKey, true, properties, body);
        // mandatory标志告诉broker代理服务器至少将消息route到一个队列中，否则就将消息return给发送者
        await channel.BasicPublishAsync(exchangeName, routingKey, false, properties, body, cancellationToken);
    }
}