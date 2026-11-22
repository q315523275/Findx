namespace Findx.Extensions.Scheduler.Abstractions;

using Findx.Extensions.Scheduler.Models;

/// <summary>
///     执行日志存储：按 TraceId 追加日志分片，供管理面分页查看完整执行过程。
///     <para>与执行账本分开存：账本行只保留尾部摘要（快查），明细表保存全量日志，避免单行被日志撑大。</para>
/// </summary>
public interface IExecutionLogStore
{
    /// <summary>
    ///     追加一批日志分片（同一批内按 Sequence 顺序写入）
    /// </summary>
    /// <param name="chunks"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>写入条数</returns>
    Task<int> AppendAsync(IReadOnlyList<ExecutionLogChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>
    ///     分页读取某次执行的日志（按主键游标，适合前端"加载更多"）
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="afterId">只取 Id 大于该值的记录，0 表示从头</param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<SchedulerExecutionLog>> ListAsync(string traceId, long afterId, int max, CancellationToken cancellationToken = default);

    /// <summary>
    ///     按 TraceId 批量删除日志明细分片（随账本一起清理）
    /// </summary>
    /// <param name="traceIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> DeleteByTraceIdsAsync(IReadOnlyList<string> traceIds, CancellationToken cancellationToken = default);
}
