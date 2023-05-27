namespace Findx.Domain;

/// <summary>
///     领域事件基类
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    /// <summary>
    ///     Ctor
    /// </summary>
    protected DomainEventBase()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    ///     编号
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     发生时间
    /// </summary>
    public DateTime OccurredOn { get; }
}