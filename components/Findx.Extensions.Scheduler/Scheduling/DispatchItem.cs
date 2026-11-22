using Findx.Extensions.Scheduler.Models;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     分发种类：周期到期 / 重试到期（两者领取语义不同）
/// </summary>
public enum DispatchKind
{
    /// <summary>
    ///     周期到期：按 Cron 推进 NextRunTime
    /// </summary>
    Cycle = 0,

    /// <summary>
    ///     重试到期：只清空 RetryAtTime，不动 NextRunTime
    /// </summary>
    Retry = 1
}

/// <summary>
///     待分发条目：调度线程从内存窗口算出的「某任务某周期的一次触发」。
///     <para>只在内存中流转；任务行 CAS 与执行行写入由分发线程的同一个事务完成，调度线程不被任何 I/O 阻塞。</para>
/// </summary>
public class DispatchItem
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public DispatchItem(ScheduledJob job, IReadOnlyList<string> periods, int attempt, DispatchKind kind, DateTime expectedTime)
    {
        Job = job;
        Periods = periods;
        Attempt = attempt;
        Kind = kind;
        ExpectedTime = expectedTime;
    }

    /// <summary>
    ///     任务定义（只读使用，不做修改）
    /// </summary>
    public ScheduledJob Job { get; }

    /// <summary>
    ///     本次要投递的周期标识集合（CatchUpWithinWindow 开启时含过去多个触发点；重试为一个 R: 前缀标识）
    /// </summary>
    public IReadOnlyList<string> Periods { get; }

    /// <summary>
    ///     主周期标识（首个，日志使用）
    /// </summary>
    public string Period => Periods.Count > 0 ? Periods[0] : string.Empty;

    /// <summary>
    ///     当前是第几次尝试（0=首次分发）
    /// </summary>
    public int Attempt { get; }

    /// <summary>
    ///     分发种类
    /// </summary>
    public DispatchKind Kind { get; }

    /// <summary>
    ///     CAS 期望值（Cycle=期望 NextRunTime，Retry=期望 RetryAtTime）
    /// </summary>
    public DateTime ExpectedTime { get; }

    /// <summary>
    ///     周期推进后的下次触发时间（仅 Cycle 有效，CAS 时一并写入）
    /// </summary>
    public DateTime NewNextRunTime { get; set; }

    /// <summary>
    ///     本次触发时刻（写入 LastRunTime，仅 Cycle 有效）
    /// </summary>
    public DateTime LastRunTime { get; set; }
}
