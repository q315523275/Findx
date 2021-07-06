using RabbitMQ.Client;
using System.Text;

namespace Findx.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConnectionPool _connectionPool;
        private readonly IRabbitMQSerializer _serializer;

        public RabbitMQPublisher(IConnectionPool connectionPool, IRabbitMQSerializer serializer)
        {
            _connectionPool = connectionPool;
            _serializer = serializer;
        }

        public void Publish(object obj, string exchangeName, string exchangeType, string routingKey)
        {
            var message = _serializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(message);

            using (var channel = _connectionPool.Acquire().CreateModel())
            {
                // 创建并配置交换器
                channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);
                // 创建队列属性
                IBasicProperties properties = channel.CreateBasicProperties();
                // 决定发送数据类型
                properties.ContentType = "application/json";
                // 是否持久化  1 no  2 yes
                properties.DeliveryMode = 2;
                // 发送数据
                channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, mandatory: true, basicProperties: properties, body: body);
            }
        }
    }
}
