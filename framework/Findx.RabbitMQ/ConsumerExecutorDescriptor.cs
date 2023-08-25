using System;
using System.Reflection;

namespace Findx.RabbitMQ
{
    /// <summary>
    ///     消费者执行信息详情
    /// </summary>
    public class ConsumerExecutorDescriptor
    {
        /// <summary>
        ///     交换机名称
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        ///     交换机类型，常见的如fanout、direct、topic
        /// </summary>
        public string ExchangeType { get; set; } = "direct";

        /// <summary>
        ///     交换机是否持久化
        /// </summary>
        public bool Durable { set; get; } = false;

        /// <summary>
        ///     队列的名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        ///     最大同时接收数量
        /// </summary>
        public int Qos { get; set; } = 1;

        /// <summary>
        ///     路由键
        /// </summary>
        public string RoutingKey { set; get; }

        /// <summary>
        ///     MQ连接名
        /// </summary>
        public string ConnectionName { set; get; }

        /// <summary>
        ///     方法
        /// </summary>
        public MethodInfo MethodInfo { set; get; }

        /// <summary>
        /// </summary>
        public Type Type { set; get; }

        /// <summary>
        /// ack消息回执
        /// </summary>
        public bool AutoAck { set; get; } = true;
    }
}