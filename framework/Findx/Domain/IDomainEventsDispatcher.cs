using System.Threading.Tasks;

namespace Findx.Domain;

/// <summary>
///     领域事件调度器
/// </summary>
public interface IDomainEventsDispatcher
{
    /// <summary>
    ///     调度所有事件
    /// </summary>
    /// <returns></returns>
    Task DispatchEventsAsync();
}