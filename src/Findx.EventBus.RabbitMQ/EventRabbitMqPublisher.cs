using Findx.EventBus.Abstractions;
using Findx.EventBus.Attributes;
using Findx.EventBus.Events;
using Findx.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace Findx.EventBus.RabbitMQ
{
    public class EventRabbitMqPublisher : IEventPublisher
    {
        private readonly IConnectionPool _connectionPool;
        private readonly IRabbitMqSerializer _rabbitMqSerializer;
        private EventBusRabbitMqOptions _mqOptions;

        public EventRabbitMqPublisher(IConnectionPool connectionPool, IRabbitMqSerializer rabbitMqSerializer, IOptionsMonitor<EventBusRabbitMqOptions> mqOptions)
        {
            _connectionPool = connectionPool;
            _rabbitMqSerializer = rabbitMqSerializer;
            _mqOptions = mqOptions.CurrentValue;
            mqOptions.OnChange(op => _mqOptions = op);
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = EventNameAttribute.GetNameOrDefault(@event.GetType());
            var message = _rabbitMqSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            using (var channel = _connectionPool.Acquire().CreateModel())
            {
                // 创建并配置交换器
                channel.ExchangeDeclare(exchange: _mqOptions.ExchangeName, type: _mqOptions.ExchangeType);
                // 创建队列属性
                IBasicProperties properties = channel.CreateBasicProperties();
                // 决定发送数据类型
                properties.ContentType = "application/json";
                // 是否持久化  1 no  2 yes
                properties.DeliveryMode = 2;
                // 发送数据
                channel.BasicPublish(exchange: _mqOptions.ExchangeName, routingKey: eventName, mandatory: true, basicProperties: properties, body: body);
            }
        }

        public Task PublishAsync(IntegrationEvent @event)
        {
            Publish(@event);
            return Task.CompletedTask;
        }
    }
}
