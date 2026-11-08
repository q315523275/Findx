using System.Threading.Tasks;
using Findx.Events;
using Findx.Messaging;

namespace Findx.Domain;

/// <summary>
///     领域事件调度器
/// </summary>
public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    /// <summary>
    ///     事件总线
    /// </summary>
    private readonly IEventBus _eventBus;

    /// <summary>
    ///     领域事件访问者
    /// </summary>
    private readonly IDomainEventsAccessor _domainEventsProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="domainEventsProvider"></param>
    /// <param name="eventBus"></param>
    public DomainEventsDispatcher(IDomainEventsAccessor domainEventsProvider, IEventBus eventBus)
    {
        _domainEventsProvider = domainEventsProvider;
        _eventBus = eventBus;
    }


    /// <summary>
    ///     调度领域事件
    /// </summary>
    /// <returns></returns>
    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsProvider.GetAllDomainEvents();

        // 全部派发成功后再清空队列：若在派发前清空，任一处理器抛异常会导致剩余事件永久丢失
        foreach (var domainEvent in domainEvents) await _eventBus.PublishAsync(domainEvent);

        _domainEventsProvider.ClearAllDomainEvents();
    }
}