using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    /// <summary>
    /// RabbitMQ消费者
    /// </summary>
    public interface IRabbitMQConsumer
    {
        /// <summary>
        /// 绑定routingKey
        /// </summary>
        /// <param name="routingKey"></param>
        void Bind(string routingKey);

        /// <summary>
        /// 解绑routingKey
        /// </summary>
        /// <param name="routingKey"></param>
        void Unbind(string routingKey);

        /// <summary>
        /// 添加消息处理
        /// </summary>
        /// <param name="callback"></param>
        void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback);

        /// <summary>
        /// 开始消费
        /// </summary>
        void StartConsuming();

        /// <summary>
        /// 异常回调
        /// </summary>
        Func<IModel, BasicDeliverEventArgs, Task> FailedCallback { set; }
    }
}
