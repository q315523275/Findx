namespace Findx.Extensions.Scheduler.Options;

/// <summary>
///     错过周期（misfire）处理策略
/// </summary>
public enum SchedulerMisfireStrategy
{
    /// <summary>
    ///     补触发一次（迟到超过一个窗口也只补一次，随后按 Cron 推进）
    /// </summary>
    FireOnceNow = 0,

    /// <summary>
    ///     只推进不触发：任务彻底错过的周期直接丢弃，不再补跑
    /// </summary>
    DoNothing = 1
}
