﻿using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     消息调度器(同步执行)
/// </summary>
[Obsolete("请使用新的“IMessageBroker”消息服务")]
public interface IMessageDispatcher
{
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete("请使用新的“IMessageBroker.SendRequestAsync”消息服务")]
    Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     推送异步执行事件
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete("请使用新的“IMessageBroker.PublishAsync”消息服务")]
    Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent;
}