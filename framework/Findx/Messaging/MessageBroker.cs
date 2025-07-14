using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Messaging;

/// <summary>
///     消息传递服务
/// </summary>
public class MessageBroker : IMessageBroker
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public MessageBroker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public Task<TResponse> SendRequestAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default)
    {
        Check.NotNull(message, nameof(message));

        var messageType = message.GetType();

        var handler = (MessageHandlerWrapper<TResponse>)MessageConst.RequestMessageHandlers.GetOrAdd(messageType
            , _ => Activator.CreateInstance(typeof(MessageHandlerWrapperImpl<,>).MakeGenericType(messageType, typeof(TResponse))));

        Check.NotNull(handler, nameof(handler));

        return handler.HandleAsync(message, _serviceProvider, cancellationToken);
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
        var eventType = applicationEvent.GetType();
        var handler = (ApplicationEventHandlerWrapper)MessageConst.ApplicationEventHandlers.GetOrAdd(eventType
            , _ => Activator.CreateInstance(typeof(ApplicationEventHandlerWrapperImpl<>).MakeGenericType(eventType)));
        
        Check.NotNull(handler, nameof(handler));
        
        return handler.HandleAsync(applicationEvent, _serviceProvider, cancellationToken);
    }
}