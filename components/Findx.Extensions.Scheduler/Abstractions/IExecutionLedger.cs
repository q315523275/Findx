namespace Findx.Extensions.Scheduler.Abstractions;

using Findx.Extensions.Scheduler.Models;

/// <summary>
///     执行账本：一次触发对应一行，状态迁移全部走列级 CAS。
///     <para>它是"至少一次投递 + 不静默丢失"的唯一事实来源：Master 崩溃后，任何节点都能从表里接管
///     未完成的行，而不是依赖丢失的进程内存。</para>
/// </summary>
public interface IExecutionLedger
{
    /// <summary>
    ///     按 TraceId 查询执行行
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SchedulerExecution> GetByTraceAsync(string traceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     受理成功标记：Claimed -> Dispatched（仅当当前状态为 Claimed 时生效）
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="workerAddress"></param>
    /// <param name="dispatchedAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> MarkDispatchedAsync(string traceId, string workerAddress, DateTime dispatchedAt, CancellationToken cancellationToken = default);

    /// <summary>
    ///     批量受理标记：一次 UPDATE 把同批已受理的行 Claimed -> Dispatched（只对当前仍为 Claimed 的行生效）
    /// </summary>
    /// <param name="traceIds">已受理的 TraceId 集合</param>
    /// <param name="workerAddress">受理节点地址</param>
    /// <param name="dispatchedAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数：小于入参数量说明部分行已被超时兜底接管，属正常竞态</returns>
    Task<int> MarkDispatchedBatchAsync(IReadOnlyList<string> traceIds, string workerAddress, DateTime dispatchedAt, CancellationToken cancellationToken = default);

    /// <summary>
    ///     终结一次执行：仅当当前状态尚未终结时生效，返回是否由本次调用终结
    ///     <para>幂等的关键所在：重复回调只会有一条成功，其余返回 false 直接忽略</para>
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="state">只接受 Succeeded / Failed / Cancelled</param>
    /// <param name="error"></param>
    /// <param name="finishedAt"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> TryFinalizeAsync(string traceId, ExecutionState state, string error, DateTime finishedAt, long? elapsedMs, CancellationToken cancellationToken = default);

    /// <summary>
    ///     终结一次执行并写入结果消息与执行过程日志（回调路径使用）
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="state">只接受 Succeeded / Failed / Cancelled</param>
    /// <param name="error"></param>
    /// <param name="finishedAt"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="message">结果消息（JobResult.Message）</param>
    /// <param name="logs">执行过程日志，调用方需自行截断</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> TryFinalizeAsync(string traceId, ExecutionState state, string error, DateTime finishedAt, long? elapsedMs,
        string message, string logs, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询未终结且分发时间早于 <paramref name="before"/> 的执行行（超时兜底扫描）
    /// </summary>
    /// <param name="before"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<SchedulerExecution>> ListStaleAsync(DateTime before, int max, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询某个任务最近的执行记录（管理面查询用）
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<SchedulerExecution>> ListByJobAsync(long jobId, int max, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询已终结且结束时间早于 <paramref name="before"/> 的执行行（归档清理用，按主键升序取一批）
    /// </summary>
    /// <param name="before"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<SchedulerExecution>> ListTerminalBeforeAsync(DateTime before, int max, CancellationToken cancellationToken = default);

    /// <summary>
    ///     按 TraceId 批量删除执行行（只删已终结的历史行，由调用方保证）
    /// </summary>
    /// <param name="traceIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> DeleteByTraceIdsAsync(IReadOnlyList<string> traceIds, CancellationToken cancellationToken = default);
}
