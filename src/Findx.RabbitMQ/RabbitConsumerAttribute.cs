using System;

namespace Findx.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RabbitConsumerAttribute : Attribute
    {
        public RabbitConsumerAttribute(string exchangeName, string type, string queueName, int qos, string routingKey)
        {
            Check.NotNull(exchangeName, nameof(exchangeName));
            Check.NotNull(type, nameof(type));
            Check.NotNull(queueName, nameof(queueName));
            Check.NotNull(routingKey, nameof(routingKey));

            ExchangeName = exchangeName;
            Type = type;
            QueueName = queueName;
            Qos = qos;
            RoutingKey = routingKey;
        }

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; }
        /// <summary>
        /// 交换机类型，常见的如fanout、direct、topic
        /// </summary>
        public string Type { get; } = "direct";
        /// <summary>
        /// 队列的名称
        /// </summary>
        public string QueueName { get; }
        /// <summary>
        /// 最大同时接收数量
        /// </summary>
        public int Qos { get; set; } = 1;
        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { set; get; }
    }
}
