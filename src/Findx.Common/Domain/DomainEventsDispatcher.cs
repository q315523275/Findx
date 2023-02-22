using System.Threading.Tasks;
using Findx.Messaging;

namespace Findx.Domain;

/// <summary>
/// 领域事件调度器
/// </summary>
public class DomainEventsDispatcher: IDomainEventsDispatcher
{
    /// <summary>
    /// 领域事件访问者
    /// </summary>
    private readonly IDomainEventsAccessor _domainEventsProvider;

    /// <summary>
    /// 消息调度器
    /// </summary>
    private readonly IMessageDispatcher _messageDispatcher;

    /// <summary>
    /// 应用事件推送者
    /// </summary>
    private readonly IApplicationEventPublisher _applicationEventPublisher;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="domainEventsProvider"></param>
    /// <param name="messageDispatcher"></param>
    /// <param name="applicationEventPublisher"></param>
    public DomainEventsDispatcher(IDomainEventsAccessor domainEventsProvider, IMessageDispatcher messageDispatcher, IApplicationEventPublisher applicationEventPublisher)
    {
        _domainEventsProvider = domainEventsProvider;
        _messageDispatcher = messageDispatcher;
        _applicationEventPublisher = applicationEventPublisher;
    }


    /// <summary>
    /// 调度领域事件
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsProvider.GetAllDomainEvents();
        
        _domainEventsProvider.ClearAllDomainEvents();
        
        foreach (var domainEvent in domainEvents)
        {
            await _applicationEventPublisher.PublishAsync(domainEvent);
        }
    }
}