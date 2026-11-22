using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.NewId;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     基于数据表的执行日志存储（scheduler_execution_log）。
///     <para>只追加不更新：日志是旁证，不允许被后续分片覆盖；读取按主键游标，天然稳定分页。</para>
/// </summary>
public class DatabaseExecutionLogStore : IExecutionLogStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyGenerator<long> _keyGenerator;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DatabaseExecutionLogStore(IServiceProvider serviceProvider, IKeyGenerator<long> keyGenerator)
    {
        _serviceProvider = serviceProvider;
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     追加日志分片
    /// </summary>
    public async Task<int> AppendAsync(IReadOnlyList<ExecutionLogChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks == null || chunks.Count == 0) return 0;

        var rows = new List<SchedulerExecutionLog>(chunks.Count);
        foreach (var chunk in chunks)
        {
            if (chunk == null || string.IsNullOrWhiteSpace(chunk.TraceId) || string.IsNullOrEmpty(chunk.Content)) continue;

            rows.Add(new SchedulerExecutionLog
            {
                //  主键显式赋雪花：scheduler_execution_log.id 无 AUTO_INCREMENT，留空会批量撞主键
                Id = _keyGenerator.Create(),
                TraceId = chunk.TraceId,
                JobId = chunk.JobId,
                Sequence = chunk.Sequence,
                Content = chunk.Content,
                IsFinal = chunk.IsFinal
            });
        }

        if (rows.Count == 0) return 0;

        return await DbAccess.UseAsync<SchedulerExecutionLog, int>(_serviceProvider,
            repo => repo.InsertAsync(rows, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     按 TraceId + 主键游标读取
    /// </summary>
    public async Task<IReadOnlyList<SchedulerExecutionLog>> ListAsync(string traceId, long afterId, int max,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(traceId)) return [];

        return await DbAccess.UseAsync<SchedulerExecutionLog, List<SchedulerExecutionLog>>(_serviceProvider,
            repo => repo.TopAsync(Math.Clamp(max, 1, 500),
                x => x.TraceId == traceId && x.Id > afterId,
                new[] { new SortCondition<SchedulerExecutionLog>(nameof(SchedulerExecutionLog.Id)) },
                cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     按 TraceId 批量删除
    /// </summary>
    public async Task<int> DeleteByTraceIdsAsync(IReadOnlyList<string> traceIds, CancellationToken cancellationToken = default)
    {
        if (traceIds == null || traceIds.Count == 0) return 0;

        var ids = traceIds.ToArray();
        return await DbAccess.UseAsync<SchedulerExecutionLog, int>(_serviceProvider,
            repo => repo.DeleteAsync(x => ids.Contains(x.TraceId), cancellationToken), cancellationToken);
    }
}
