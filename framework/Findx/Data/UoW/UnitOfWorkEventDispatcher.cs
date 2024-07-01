using System.Threading.Tasks;
using Findx.Events;

namespace Findx.Data;

/// <summary>
///     工作单元事件调度器
/// </summary>
public class UnitOfWorkEventDispatcher: IUnitOfWorkEventDispatcher
{
    private readonly ConcurrentDictionary<TransactionPhase, List<IEvent>> _eventDict = new();
    private readonly IEventBus _eventBus;
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public UnitOfWorkEventDispatcher(IServiceProvider serviceProvider)
    {
        _eventBus = serviceProvider.GetRequiredService<IEventBus>();
    }

    /// <summary>
    ///     添加事件至缓冲区
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="transactionPhase"></param>
    /// <typeparam name="TEvent"></typeparam>
    public void AddEventToBuffer<TEvent>(TEvent eventData, TransactionPhase transactionPhase = TransactionPhase.AfterCommit) where TEvent : IEvent
    {
        if (!_eventDict.ContainsKey(transactionPhase))
        {
            _eventDict[transactionPhase] = [];
        }
        _eventDict[transactionPhase].Add(eventData);
    }

    /// <summary>
    ///     触发事件
    /// </summary>
    /// <param name="transactionPhase"></param>
    /// <param name="cancellationToken"></param>
    public async Task ProcessEventAsync(TransactionPhase transactionPhase, CancellationToken cancellationToken = default)
    {
        if (_eventDict.TryGetValue(transactionPhase, out var events))
        {
            foreach (var eventData in events)
            {
                await _eventBus.PublishAsync(eventData, cancellationToken);
            }
        }
    }

    /// <summary>
    ///     清除
    /// </summary>
    public void ClearAllEvents()
    {
        _eventDict.Clear();
    }
}