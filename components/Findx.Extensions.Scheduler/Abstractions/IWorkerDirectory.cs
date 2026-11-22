namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     Worker 节点目录：执行端心跳上报，调度端按能力反查可执行节点。
///     <para>默认实现落库；若后续需要 Redis 版，只需替换实现，调度侧读取路径不变。</para>
/// </summary>
public interface IWorkerDirectory
{
    /// <summary>
    ///     写入/刷新一个节点的心跳（按 WorkerId 幂等覆盖）
    /// </summary>
    /// <param name="worker"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HeartbeatAsync(Models.WorkerNodeInfo worker, CancellationToken cancellationToken = default);

    /// <summary>
    ///     摘除一个节点（优雅下线时调用，不必等 TTL 过期）
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveAsync(string workerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询心跳时间不早于 <paramref name="since"/> 的存活节点
    /// </summary>
    /// <param name="since"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<Models.WorkerNodeInfo>> ListAliveAsync(DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    ///     清理心跳早于 <paramref name="before"/> 的节点记录，返回清理条数
    /// </summary>
    /// <param name="before"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> PruneAsync(DateTime before, CancellationToken cancellationToken = default);
}
