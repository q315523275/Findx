using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Messaging;

/// <summary>
///     消息传递服务
/// </summary>
public class MessageDispatcher : IMessageDispatcher
{
    private readonly IMessageBroker _messageBroker;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="messageBroker"></param>
    public MessageDispatcher(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default)
    {
        return _messageBroker.SendRequestAsync(message, cancellationToken);
    }

    /// <summary>
    ///     发布应用事件
    /// </summary>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    public Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent
    {
        return _messageBroker.PublishAsync(applicationEvent, cancellationToken: cancellationToken);
    }
}