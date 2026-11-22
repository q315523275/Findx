using Findx.Extensions.Scheduler.Models;

namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     Worker → 调度端的上行协议通道。
///     <para>执行端不持有任何数据库连接，所有登记 / 结果 / 日志都通过本接口经协议上送。
///     当前实现为 HTTP（<c>HttpSchedulerMasterChannel</c>）；后续扩展 gRPC / 消息队列时，
///     只需新增一个实现并在模块里替换注册，执行端其余代码不感知协议。</para>
/// </summary>
public interface ISchedulerMasterChannel
{
    /// <summary>
    ///     协议标识（http / grpc / mq ...），用于日志与运维排障
    /// </summary>
    string Protocol { get; }

    /// <summary>
    ///     心跳上报：把本节点地址与任务清单登记到调度端
    /// </summary>
    Task<bool> HeartbeatAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default);

    /// <summary>
    ///     下线通知：请求调度端立即摘除本节点
    /// </summary>
    Task<bool> OfflineAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行结果回报。地址优先用分发包携带的调度端地址，其次取配置的候选地址
    /// </summary>
    Task<bool> ReportResultAsync(JobResultReport report, string preferredMasterAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行结果批量回报：多条结果合并成一次上行请求。
    ///     <para>语义是「送达」：调度端按 TraceId 幂等终结，因此整批重试只会命中已终结的行并被忽略。</para>
    /// </summary>
    Task<bool> ReportResultsAsync(string preferredMasterAddress, IReadOnlyList<JobResultReport> reports,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行过程日志分片批量上报。调用方保证同一批分片属于同一次执行
    /// </summary>
    Task<bool> ShipLogsAsync(string preferredMasterAddress, IReadOnlyList<ExecutionLogChunk> chunks,
        CancellationToken cancellationToken = default);
}
