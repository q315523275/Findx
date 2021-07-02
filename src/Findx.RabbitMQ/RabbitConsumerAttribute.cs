using System;

namespace Findx.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RabbitConsumerAttribute : Attribute
    {
        /// <summary>
        /// Exchange配置
        /// </summary>
        public ExchangeDeclareConfiguration Exchange { set; get; }
        /// <summary>
        /// Queue配置
        /// </summary>
        public QueueDeclareConfiguration Queue { set; get; }
        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { set; get; }
        /// <summary>
        /// 消费消息
        /// </summary>
        public Type Clazz { set; get; }
    }
}
