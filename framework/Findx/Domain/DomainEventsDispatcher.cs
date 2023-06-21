using System.Threading.Tasks;
using Findx.Messaging;

namespace Findx.Domain;

/// <summary>
///     领域事件调度器
/// </summary>
public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    /// <summary>
    /// 消息调度
    /// </summary>
    private readonly IMessageDispatcher _messageDispatcher;

    /// <summary>
    ///     领域事件访问者
    /// </summary>
    private readonly IDomainEventsAccessor _domainEventsProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="domainEventsProvider"></param>
    /// <param name="messageDispatcher"></param>
    public DomainEventsDispatcher(IDomainEventsAccessor domainEventsProvider, IMessageDispatcher messageDispatcher)
    {
        _domainEventsProvider = domainEventsProvider;
        _messageDispatcher = messageDispatcher;
    }


    /// <summary>
    ///     调度领域事件
    /// </summary>
    /// <returns></returns>
    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsProvider.GetAllDomainEvents();

        _domainEventsProvider.ClearAllDomainEvents();

        foreach (var domainEvent in domainEvents) await _messageDispatcher.PublishAsync(domainEvent);
    }
}