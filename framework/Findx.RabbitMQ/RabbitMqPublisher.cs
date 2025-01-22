using System.Text;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ推送者
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConnectionPool _connectionPool;
    private readonly IRabbitMqSerializer _serializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="serializer"></param>
    public RabbitMqPublisher(IConnectionPool connectionPool, IRabbitMqSerializer serializer)
    {
        _connectionPool = connectionPool;
        _serializer = serializer;
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
    public void Publish(object obj, string exchangeName, string exchangeType, string routingKey, bool durable = false, bool autoDelete = false)
    {
        var message = _serializer.Serialize(obj);
        var body = Encoding.UTF8.GetBytes(message);

        using var channel = _connectionPool.Get().CreateModel();
        // 创建并配置交换器
        channel.ExchangeDeclare(exchangeName, exchangeType, durable: durable, autoDelete: autoDelete);
        // 创建队列属性
        var properties = channel.CreateBasicProperties();
        // 决定发送数据类型
        properties.ContentType = "application/json";
        // 是否持久化  1 no  2 yes
        properties.DeliveryMode = 2;
        // 发送数据
        channel.BasicPublish(exchangeName, routingKey, true, properties, body);
    }
}