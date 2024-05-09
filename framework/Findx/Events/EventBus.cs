using System.Threading.Tasks;
using Findx.Common;
using Findx.Messaging;

namespace Findx.Events;

/// <summary>
///     事件总线
/// </summary>
public class EventBus: IEventBus
{
    private readonly IApplicationEventPublisher _eventPublisher;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly IDistributedEventBus _distributedEventBus;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="eventPublisher"></param>
    /// <param name="messageDispatcher"></param>
    /// <param name="distributedEventBus"></param>
    public EventBus(IApplicationEventPublisher eventPublisher, IMessageDispatcher messageDispatcher, IDistributedEventBus distributedEventBus = null)
    {
        _eventPublisher = eventPublisher;
        _messageDispatcher = messageDispatcher;
        _distributedEventBus = distributedEventBus;
    }

    /// <summary>
    ///     推送事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        
        // net7
        // ArgumentNullException.ThrowIfNull(@event, nameof(@event));
        // 分发
        
        return @event switch
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            IApplicationEvent applicationEvent => applicationEvent is IAsync
                ? _eventPublisher.PublishAsync(applicationEvent, cancellationToken)
                : _messageDispatcher.PublishAsync(applicationEvent, cancellationToken),
            IIntegrationEvent integrationEvent => _distributedEventBus?.PublishAsync(integrationEvent,
                cancellationToken),
            _ => throw new Exception($"no matching sender found for the event ({nameof(@event)})")
        };
    }
}