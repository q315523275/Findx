using Findx.Extensions.Scheduler.Models;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     轮询分发策略：按 JobName 维护独立游标依次轮转。
///     <para>游标是热路径：每次投递都要推进一次。用「GetOrAdd 一个计数器 + Interlocked」替代
///     <c>ConcurrentDictionary.AddOrUpdate</c>——后者每次都是一次字典写（含 bucket 锁与 CAS 重试），
///     500 次/秒触发下就是每秒上千次写；前者只有首次创建走字典，之后只是原子自增。</para>
///     <para>扩展位：WorkerNodeInfo.Weight 已预留，后续可替换为加权随机策略而不改动调用方。</para>
/// </summary>
public class RoundRobinDispatchStrategy : IDispatchStrategy
{
    private readonly ConcurrentDictionary<string, Cursor> _cursors = new();

    /// <summary>
    ///     选择节点
    /// </summary>
    /// <param name="candidates"></param>
    /// <param name="jobKey"></param>
    /// <returns></returns>
    public WorkerNodeInfo Select(IReadOnlyList<WorkerNodeInfo> candidates, string jobKey)
    {
        if (candidates == null || candidates.Count == 0) return null;

        var cursor = _cursors.GetOrAdd(jobKey ?? string.Empty, static _ => new Cursor());

        //  游标自增后用无符号取模：避免 int 溢出到负数时索引越界
        var next = (uint)cursor.Next();
        return candidates[(int)(next % (uint)candidates.Count)];
    }

    /// <summary>
    ///     单任务的轮询游标（只做原子自增，不参与字典写）
    /// </summary>
    private sealed class Cursor
    {
        private int _value;

        /// <summary>
        ///     取下一个序号
        /// </summary>
        /// <returns></returns>
        public int Next() => Interlocked.Increment(ref _value);
    }
}
