namespace Findx.Extensions.Scheduler.Abstractions;

using Findx.Extensions.Scheduler.Models;

/// <summary>
///     周期领取器：把"推进任务锚点"与"写入执行行"放进同一个事务，
///     消除两次写入之间的崩溃窗口（否则会出现"任务已推进但没有执行行"的静默丢失）。
/// </summary>
public interface IPeriodClaimer
{
    /// <summary>
    ///     领取一个周期触发点（可能一次领取多个补触发点）
    /// </summary>
    /// <param name="job">任务定义（读取 Id / 名称 / 类型 / 参数）</param>
    /// <param name="periods">本次要投递的周期标识</param>
    /// <param name="attempt">当前尝试序号（0=首次）</param>
    /// <param name="expectedNextRunTime">CAS 期望的 NextRunTime</param>
    /// <param name="newNextRunTime">CAS 写入的下一次触发时间</param>
    /// <param name="lastRunTime">CAS 写入的本次触发时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回执行行集合；被其他节点抢先返回 null</returns>
    Task<List<SchedulerExecution>> TryClaimCycleAsync(ScheduledJob job, IReadOnlyList<string> periods, int attempt, DateTime expectedNextRunTime, DateTime newNextRunTime
        , DateTime lastRunTime, CancellationToken cancellationToken = default);

    /// <summary>
    ///     批量领取：一批到期任务共用同一个事务与一次提交。
    ///     <para>逐行 CAS 仍然是每行独立判断，只有事务级异常才整批回滚；
    ///     目的是把「每次领取一次事务提交（含刷盘）」摊薄到一条批量提交上。</para>
    /// </summary>
    /// <param name="requests">按输入顺序返回结果，调用方可用下标一一对应</param>
    /// <param name="cancellationToken"></param>
    /// <returns>与 requests 等长的结果集合</returns>
    Task<IReadOnlyList<ClaimBatchResult>> TryClaimBatchAsync(IReadOnlyList<ClaimBatchRequest> requests, CancellationToken cancellationToken = default);

    /// <summary>
    ///     领取一次重试：CAS 清空 RetryAtTime，并写入执行行
    /// </summary>
    /// <param name="job"></param>
    /// <param name="period">重试周期标识（R: 前缀）</param>
    /// <param name="attempt">本次尝试序号（1-based，第 1 次重试=1）</param>
    /// <param name="expectedRetryAtTime">CAS 期望的 RetryAtTime</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回执行行；被其他节点抢先返回 null</returns>
    Task<SchedulerExecution> TryClaimRetryAsync(ScheduledJob job, string period, int attempt, DateTime expectedRetryAtTime, CancellationToken cancellationToken = default);

    /// <summary>
    ///     手动触发：不推进周期锚点，仅创建一条执行行
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SchedulerExecution> OpenManualAsync(ScheduledJob job, CancellationToken cancellationToken = default);
}
