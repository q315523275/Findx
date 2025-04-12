using System;
using System.Threading;
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
    Func<IChannel, BasicDeliverEventArgs, CancellationToken, Task> FailedCallback { set; }

    /// <summary>
    ///     绑定routingKey
    /// </summary>
    /// <param name="routingKey"></param>
    /// <param name="cancellationToken"></param>
    Task BindAsync(string routingKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     解绑routingKey
    /// </summary>
    /// <param name="routingKey"></param>
    /// <param name="cancellationToken"></param>
    Task UnbindAsync(string routingKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加消息处理
    /// </summary>
    /// <param name="callback"></param>
    void OnMessageReceived(Func<IChannel, BasicDeliverEventArgs, Task> callback);
}