using Microsoft.Extensions.Options;

namespace Findx.EventBus.RabbitMQ
{
    public class EventBusRabbitMqOptions : IOptions<EventBusRabbitMqOptions>
    {
        public EventBusRabbitMqOptions Value => this;
        /// <summary>
        /// 交换器名
        /// </summary>
        public string ExchangeName { set; get; } = "findx_event_bus";
        /// <summary>
        /// 路由方式
        /// </summary>
        public string ExchangeType { set; get; } = "direct";

        /// <summary>
        /// 消费端队列名
        /// </summary>
        public string QueueName { set; get; }
        /// <summary>
        /// 消费端预取消息数量
        /// </summary>
        public int PrefetchCount { set; get; } = 1;
    }
}
