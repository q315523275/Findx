#nullable enable
using System.Threading.Tasks;
using Findx.Data;

namespace Findx.Events;

/// <summary>
///     分布式消息总线
/// </summary>
public interface IDistributedEventBus
{
    /// <summary>
    ///     工作单元
    /// </summary>
    IUnitOfWork? UnitOfWork { get; set; }

    /// <summary>
    ///     推送事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;

    /// <summary>
    ///     事件订阅
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TDistributedEventHandler"></typeparam>
    /// <returns></returns>
    IDisposable Subscribe<TEvent, TDistributedEventHandler>() where TEvent : IIntegrationEvent where TDistributedEventHandler : IDistributedEventHandler<IIntegrationEvent>;
    
    /// <summary>
    ///     取消订阅
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TDistributedEventHandler"></typeparam>
    /// <returns></returns>
    IDisposable UnSubscribe<TEvent, TDistributedEventHandler>() where TEvent : IIntegrationEvent where TDistributedEventHandler : IDistributedEventHandler<IIntegrationEvent>;
}