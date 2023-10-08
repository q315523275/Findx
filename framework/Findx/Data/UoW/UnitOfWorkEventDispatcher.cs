using System.Threading.Tasks;
using Findx.Common;
using Findx.Events;
using Findx.Messaging;

namespace Findx.Data;

/// <summary>
/// 工作单元事件调度器
/// </summary>
public class UnitOfWorkEventDispatcher: IUnitOfWorkEventDispatcher
{
    private readonly List<IApplicationEvent> _applicationEvents = new();
    private readonly List<IApplicationEvent> _applicationAsyncEvents = new();
    private readonly List<IIntegrationEvent> _distributedEvents = new();
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly IApplicationEventPublisher _applicationEventPublisher;
    private readonly IDistributedEventBus _distributedEventBus;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public UnitOfWorkEventDispatcher(IServiceProvider serviceProvider)
    {
        _messageDispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();
        _applicationEventPublisher = serviceProvider.GetRequiredService<IApplicationEventPublisher>();
        _distributedEventBus = serviceProvider.GetService<IDistributedEventBus>();
    }
    
    
    /// <summary>
    /// 添加事件至缓冲区
    /// </summary>
    /// <param name="eventData"></param>
    /// <typeparam name="TEvent"></typeparam>
    public void AddEventToBuffer<TEvent>(TEvent eventData) where TEvent : IEvent
    {
        switch (eventData)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            case IApplicationEvent localEvent when eventData is IAsync:
                _applicationAsyncEvents.Add(localEvent);
                break;
            case IApplicationEvent localEvent:
                _applicationEvents.Add(localEvent);
                break;
            case IIntegrationEvent integrationEvent:
                _distributedEvents.Add(integrationEvent);
                break;
        }
    }

    /// <summary>
    /// 推送事件
    /// <para />
    /// 同步执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PublishEventsAsync(CancellationToken cancellationToken)
    {
        foreach (var applicationEvent in _applicationEvents)
            await _messageDispatcher.PublishAsync(applicationEvent, cancellationToken);
        _applicationEvents.Clear();
    }

    /// <summary>
    /// 推送异步事件
    ///  <para />
    /// 异步线程执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task PublishAsyncEventsAsync(CancellationToken cancellationToken)
    {
        foreach (var applicationEvent in _applicationAsyncEvents)
            await _applicationEventPublisher.PublishAsync(applicationEvent, cancellationToken);
        
        _applicationAsyncEvents.Clear();
        
        if (_distributedEventBus != null)
        {
            foreach (var integrationEvent in _distributedEvents)
                await _distributedEventBus.PublishAsync(integrationEvent, cancellationToken);
        }
        
        _distributedEvents.Clear();
    }

    /// <summary>
    /// 清除
    /// </summary>
    public void ClearAllEvents()
    {
        _applicationEvents.Clear();
        _applicationAsyncEvents.Clear();
        _distributedEvents.Clear();
    }
}