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

        _domainEventsProvider.ClearAllDomainEvents();

        foreach (var domainEvent in domainEvents) await _eventBus.PublishAsync(domainEvent);
    }
}