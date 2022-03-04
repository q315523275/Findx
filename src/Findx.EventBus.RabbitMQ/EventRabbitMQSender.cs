using Findx.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus.RabbitMQ
{
    public class EventRabbitMQSender : IEventSender
    {
        private readonly IConnectionPool _pool;
        private IOptionsMonitor<EventBusRabbitMqOptions> _options;

        public EventRabbitMQSender(IConnectionPool pool, IOptionsMonitor<EventBusRabbitMqOptions> options)
        {
            _pool = pool;
            _options = options;
        }

        public void Send(TransportMessage message)
        {
            using (var channel = _pool.Acquire().CreateModel())
            {
                // 创建并配置交换器
                channel.ExchangeDeclare(exchange: _options.CurrentValue.ExchangeName, type: _options.CurrentValue.ExchangeType);
                // 创建队列属性
                IBasicProperties properties = channel.CreateBasicProperties();
                // 决定发送数据类型
                properties.ContentType = "application/json";
                // 是否持久化  1 no  2 yes
                properties.DeliveryMode = 2;
                // 头信息
                properties.Headers = message.Headers.ToDictionary(x => x.Key, x => (object)x.Value);
                // 发送数据
                channel.BasicPublish(exchange: _options.CurrentValue.ExchangeName, routingKey: message.GetEventName(), mandatory: true, basicProperties: properties, body: message.Body);
            }
        }

        public Task SendAsync(TransportMessage message, CancellationToken cancellationToken = default)
        {
            using (var channel = _pool.Acquire().CreateModel())
            {
                // 创建并配置交换器
                channel.ExchangeDeclare(exchange: _options.CurrentValue.ExchangeName, type: _options.CurrentValue.ExchangeType);
                // 创建队列属性
                IBasicProperties properties = channel.CreateBasicProperties();
                // 决定发送数据类型
                properties.ContentType = "application/json";
                // 是否持久化  1 no  2 yes
                properties.DeliveryMode = 2;
                // 头信息
                properties.Headers = message.Headers.ToDictionary(x => x.Key, x => (object)x.Value);
                // 发送数据
                channel.BasicPublish(exchange: _options.CurrentValue.ExchangeName, routingKey: message.GetEventName(), mandatory: true, basicProperties: properties, body: message.Body);
            }

            return Task.CompletedTask;
        }
    }
}
