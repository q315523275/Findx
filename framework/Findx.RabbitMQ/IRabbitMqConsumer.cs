using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Findx.RabbitMQ;

/// <summary>
///     RabbitMQ消费者
/// </summary>
public interface IRabbitMqConsumer
{
    /// <summary>
    ///     异常回调
    /// </summary>
    Func<IModel, BasicDeliverEventArgs, Task> FailedCallback { set; }

    /// <summary>
    ///     绑定routingKey
    /// </summary>
    /// <param name="routingKey"></param>
    Task BindAsync(string routingKey);

    /// <summary>
    ///     解绑routingKey
    /// </summary>
    /// <param name="routingKey"></param>
    Task UnbindAsync(string routingKey);

    /// <summary>
    ///     添加消息处理
    /// </summary>
    /// <param name="callback"></param>
    void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback);
}