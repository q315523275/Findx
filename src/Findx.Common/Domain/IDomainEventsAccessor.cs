namespace Findx.Domain;

/// <summary>
/// 领域事件访问者
/// </summary>
public interface IDomainEventsAccessor
{
    /// <summary>
    /// 获取所有领域事件
    /// </summary>
    /// <returns></returns>
    IReadOnlyCollection<IDomainEvent> GetAllDomainEvents();

    /// <summary>
    /// 清除领域事件
    /// </summary>
    void ClearAllDomainEvents();
}