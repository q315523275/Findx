using System;

namespace Findx.RabbitMQ
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RabbitConsumerAttribute : Attribute
    {
        public RabbitConsumerAttribute(string exchangeName, string exchangeType, string queueName, int qos, string routingKey, string connectionName = null)
        {
            Check.NotNull(exchangeName, nameof(exchangeName));
            Check.NotNull(exchangeType, nameof(exchangeType));
            Check.NotNull(queueName, nameof(queueName));
            Check.NotNull(routingKey, nameof(routingKey));

            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            QueueName = queueName;
            Qos = qos;
            RoutingKey = routingKey;
            ConnectionName = connectionName;
        }

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; }

        /// <summary>
        /// 交换机类型，常见的如fanout、direct、topic
        /// </summary>
        public string ExchangeType { get; } = "direct";

        /// <summary>
        /// 交换机是否持久化
        /// </summary>
        public bool Durable { set; get; } = false;

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

        /// <summary>
        /// MQ连接名
        /// </summary>
        public string ConnectionName { set; get; }
    }
}
