using System.Collections.Generic;
using Findx.Common;
using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    /// <summary>
    ///     队列声明配置
    /// </summary>
    public class QueueDeclareConfiguration
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="qos"></param>
        /// <param name="autoAck"></param>
        public QueueDeclareConfiguration(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, int qos = 1, bool autoAck = true)
        {
            Check.NotNull(queueName, nameof(queueName));

            QueueName = queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            Qos = qos;
            Arguments = new Dictionary<string, object>();
            AutoAck = autoAck;
        }

        /// <summary>
        ///     队列的名称
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        ///     是否持久化
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        ///     是否排他的
        ///     如果一个队列声明为排他队列，该队列公对首次声明它的连接可见，并在连接断开时自动删除
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        ///     是否自动删除
        ///     至少有一个消息者连接到这个队列，之后所有与这个队列连接的消息都断开时，才会自动删除
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        ///     最大同时接收数量
        /// </summary>
        public int Qos { get; set; }

        /// <summary>
        ///     设置队列的其他一些参数，如x-message-ttl等
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }

        /// <summary>
        ///     是否自动确认消费，无异常自动ack
        /// </summary>
        public bool AutoAck { get; set; }

        /// <summary>
        ///     队列声明
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public virtual QueueDeclareOk Declare(IModel channel)
        {
            return channel.QueueDeclare(QueueName, Durable, Exclusive, AutoDelete, Arguments);
        }
    }
}