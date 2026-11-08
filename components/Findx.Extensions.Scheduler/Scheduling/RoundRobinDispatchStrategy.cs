using Findx.Extensions.Scheduler.Models;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     轮询分发策略：按 JobName 维护独立游标依次轮转。
///     <para>扩展位：WorkerNodeInfo.Weight 已预留，后续可替换为加权随机策略而不改动调用方。</para>
/// </summary>
public class RoundRobinDispatchStrategy : IDispatchStrategy
{
    private readonly ConcurrentDictionary<string, int> _cursors = new();

    /// <summary>
    ///     选择节点
    /// </summary>
    public WorkerNodeInfo Select(IReadOnlyList<WorkerNodeInfo> candidates, string jobKey)
    {
        if (candidates == null || candidates.Count == 0) return null;

        var key = jobKey ?? string.Empty;
        var next = _cursors.AddOrUpdate(key, 0, (_, current) => current + 1);
        var index = (int)(Math.Abs((long)next) % candidates.Count);
        return candidates[index];
    }
}
