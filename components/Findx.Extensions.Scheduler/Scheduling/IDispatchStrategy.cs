using Findx.Extensions.Scheduler.Models;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     分发策略：从某任务的可执行 Worker 列表中选一个节点
/// </summary>
public interface IDispatchStrategy
{
    /// <summary>
    ///     选择节点
    /// </summary>
    /// <param name="candidates">可执行该 Job 的健康 Worker 列表（调用方已过滤）</param>
    /// <param name="jobKey">任务键（JobName），策略据此维持轮转游标</param>
    /// <returns>选中的节点；候选为空返回 null</returns>
    WorkerNodeInfo Select(IReadOnlyList<WorkerNodeInfo> candidates, string jobKey);
}
