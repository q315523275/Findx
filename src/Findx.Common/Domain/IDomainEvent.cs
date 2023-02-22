using Findx.Messaging;

namespace Findx.Domain;

/// <summary>
/// 领域事件
/// </summary>
public interface IDomainEvent: IApplicationEvent
{
    /// <summary>
    /// 编号
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// 发生时间
    /// </summary>
    DateTime OccurredOn { get; }
}