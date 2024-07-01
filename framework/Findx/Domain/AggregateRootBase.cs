using Findx.Data;

namespace Findx.Domain;

/// <summary>
///     聚合根基类
/// </summary>
public abstract class AggregateRootBase : EntityBase<Guid>, IAggregateRoot
{
    private List<IDomainEvent> _domainEvents;

    /// <summary>
    ///     领域事件集合
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

    /// <summary>
    ///     清除领域事件集合
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    ///     添加领域事件
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];

        _domainEvents.Add(domainEvent);
    }
}

/// <summary>
///     聚合根基类
/// </summary>
public abstract class AggregateRootBase<TKey> : EntityBase<TKey>, IAggregateRoot where TKey : IEquatable<TKey>
{
    private List<IDomainEvent> _domainEvents;

    /// <summary>
    ///     领域事件集合
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

    /// <summary>
    ///     清除领域事件集合
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    ///     添加领域事件
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];

        _domainEvents.Add(domainEvent);
    }
}