using Findx.Extensions.Scheduler.Scheduling;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     批量领取中的单条请求：对应一个到期任务的一次领取（周期领取或重试领取）。
/// </summary>
public class ClaimBatchRequest
{
    /// <summary>
    ///     任务定义（只读，用于取 Id / 名称 / 类型 / 参数）
    /// </summary>
    public ScheduledJob Job { get; set; }

    /// <summary>
    ///     本次要投递的周期标识（补触发时可能多个；重试为一个 R: 前缀标识）
    /// </summary>
    public IReadOnlyList<string> Periods { get; set; }

    /// <summary>
    ///     当前尝试序号
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    ///     领取种类：<see cref="DispatchKind.Cycle" /> 推进周期锚点；<see cref="DispatchKind.Retry" /> 只清 RetryAtTime
    /// </summary>
    public DispatchKind Kind { get; set; }

    /// <summary>
    ///     CAS 期望值（Cycle=NextRunTime，Retry=RetryAtTime）
    /// </summary>
    public DateTime ExpectedTime { get; set; }

    /// <summary>
    ///     周期推进后的下次触发时间（Cycle 有效）
    /// </summary>
    public DateTime NewNextRunTime { get; set; }

    /// <summary>
    ///     本次触发时刻，写入 LastRunTime（Cycle 有效）
    /// </summary>
    public DateTime LastRunTime { get; set; }
}

/// <summary>
///     批量领取中的单条结果。
///     <para>整批共享一个事务与一次提交：任一行 CAS 未命中只影响该行，其余行照常落库；
///     只有事务级异常（连接中断 / 唯一键冲突）才会整批回滚，此时全部按「未领取」处理。</para>
/// </summary>
public class ClaimBatchResult
{
    /// <summary>
    ///     对应的请求
    /// </summary>
    public ClaimBatchRequest Request { get; set; }

    /// <summary>
    ///     成功时返回的执行行（补触发可能多条）；失败为 null
    /// </summary>
    public List<SchedulerExecution> Executions { get; set; }

    /// <summary>
    ///     是否领取成功
    /// </summary>
    public bool Succeeded => Executions is { Count: > 0 };
}
