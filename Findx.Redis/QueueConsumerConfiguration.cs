using System.Collections.Generic;

namespace Findx.Redis
{
    /// <summary>
    /// 队列声明配置
    /// </summary>
    public class QueueConsumerConfiguration
    {
        /// <summary>
        /// 组的名称
        /// </summary>
        public string GroupName { get; }
        /// <summary>
        /// 队列的名称
        /// </summary>
        public string QueueName { get; }
        /// <summary>
        /// 消费的名称
        /// </summary>
        public string ConsumerName { get; }
        /// <summary>
        /// 最大同时接收数量
        /// </summary>
        public int Qos { get; set; }
        /// <summary>
        /// 设置队列的其他一些参数，如x-message-ttl等
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="queueName"></param>
        /// <param name="qos"></param>
        public QueueConsumerConfiguration(string queueName, string groupName, string consumerName, int qos = 1)
        {
            Check.NotNull(queueName, nameof(queueName));

            GroupName = groupName;
            QueueName = queueName;
            ConsumerName = consumerName;
            Qos = qos;
            Arguments = new Dictionary<string, object>();
        }
    }
}
