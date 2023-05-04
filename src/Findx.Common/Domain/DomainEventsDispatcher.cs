using System.Threading.Tasks;
using Findx.Messaging;

namespace Findx.Domain;

/// <summary>
///     领域事件调度器
/// </summary>
public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    /// <summary>
    ///     应用事件推送者
    /// </summary>
    private readonly IApplicationEventPublisher _applicationEventPublisher;

    /// <summary>
    ///     领域事件访问者
    /// </summary>
    private readonly IDomainEventsAccessor _domainEventsProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="domainEventsProvider"></param>
    /// <param name="applicationEventPublisher"></param>
    public DomainEventsDispatcher(IDomainEventsAccessor domainEventsProvider,
        IApplicationEventPublisher applicationEventPublisher)
    {
        _domainEventsProvider = domainEventsProvider;
        _applicationEventPublisher = applicationEventPublisher;
    }


    /// <summary>
    ///     调度领域事件
    /// </summary>
    /// <returns></returns>
    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsProvider.GetAllDomainEvents();

        _domainEventsProvider.ClearAllDomainEvents();

        foreach (var domainEvent in domainEvents) await _applicationEventPublisher.PublishAsync(domainEvent);
    }
}