using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     调度端节点注册表（只存在于调度端进程）。
///     <para>执行端按心跳间隔通过 <c>ISchedulerMasterChannel</c> 上报自身能力（<c>POST /api/scheduler/workers/beat</c>）；
///     调度端把心跳落库并周期刷新本地缓存，按 HandlerType 反查可执行节点。</para>
///     <para>本类不被注册到执行端容器：执行端没有数据库连接，登记全部走协议。</para>
/// </summary>
public class WorkerRegistry : BackgroundService
{
    private readonly ILogger<WorkerRegistry> _logger;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IWorkerDirectory _directory;
    private readonly IClusterClock _clock;

    /// <summary>
    ///     节点视图快照：一次刷新构建、原子替换。
    ///     <para>节点数与能力清单只在刷新时变化，读侧（每次投递都要选点）必须是免遍历、免分配的：
    ///     之前每次 GetCandidates 都会对全部节点 × 全部能力做线性比较并 ToList，
    ///     500 次/秒触发下就是每秒上千次列表分配。</para>
    /// </summary>
    private volatile WorkerView _view = new();

    /// <summary>
    ///     WorkerId -> 节点信息（调度端缓存）
    /// </summary>
    private ConcurrentDictionary<string, WorkerNodeInfo> Workers => _view.Workers;

    /// <summary>
    ///     Ctor
    /// </summary>
    public WorkerRegistry(IOptions<SchedulerOptions> options, IWorkerDirectory directory, IClusterClock clock,
        ILogger<WorkerRegistry> logger)
    {
        _options = options;
        _directory = directory;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    ///     主循环：周期读表刷新快照 + 清理过期行
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshWorkerMapAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                //  刷新失败不清空快照：沿用上一次快照，避免一次数据库抖动把所有节点判成不可用
                _logger.LogError(ex, "调度端刷新节点注册表失败，沿用上一次快照（{Count} 个节点）", _view.Workers.Count);
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.Value.WorkerHeartbeatIntervalSeconds)),
                stoppingToken);
        }
    }

    /// <summary>
    ///     刷新节点缓存：读表 + 清理过期行 + 原子替换快照
    /// </summary>
    private async Task RefreshWorkerMapAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        var ttl = TimeSpan.FromSeconds(Math.Max(1, opts.WorkerHeartbeatTtlSeconds));
        var since = _clock.Now.Subtract(ttl);

        var alive = await _directory.ListAliveAsync(since, cancellationToken);
        var snapshot = new Dictionary<string, WorkerNodeInfo>(alive.Count);
        foreach (var worker in alive)
        {
            if (worker == null || string.IsNullOrWhiteSpace(worker.WorkerId)) continue;
            snapshot[worker.WorkerId] = worker;
        }

        //  原子替换：读侧要么看到完整旧快照，要么看到完整新快照
        var view = new WorkerView
        {
            Workers = new ConcurrentDictionary<string, WorkerNodeInfo>(snapshot),
            ByCapability = BuildCapabilityIndex(snapshot)
        };
        _view = view;

        //  顺手清理过期行，避免注册表只增不减
        await _directory.PruneAsync(since, cancellationToken);
    }

    /// <summary>
    ///     存活节点数（快照读，无锁）
    /// </summary>
    public int AliveWorkerCount => _view.Workers.Count;

    /// <summary>
    ///     当前存活节点快照（管理面展示节点任务清单用）
    /// </summary>
    public IReadOnlyList<WorkerNodeInfo> Snapshot()
    {
        return _view.Workers.Values.ToList();
    }

    /// <summary>
    ///     反查可执行该任务的健康 Worker 列表（O(1) 字典查找，返回共享数组，不分配）。
    ///     <para>以 <c>HandlerType</c>（IJob 实现类全名）为准：执行端按全名解析实现，按 JobName 匹配会把任务投到
    ///     没部署该实现的节点，那里只能失败。仅在未配置 HandlerType 时才退化为按 JobName 匹配。</para>
    ///     <para>返回的列表是刷新时预构建的只读数组，调用方只读遍历，不要修改。</para>
    /// </summary>
    public IReadOnlyList<WorkerNodeInfo> GetCandidates(string jobName, string handlerType)
    {
        var key = string.IsNullOrWhiteSpace(handlerType) ? jobName : handlerType;
        if (string.IsNullOrWhiteSpace(key)) return Array.Empty<WorkerNodeInfo>();

        return _view.ByCapability.TryGetValue(key, out var workers) ? workers : Array.Empty<WorkerNodeInfo>();
    }

    /// <summary>
    ///     构建「能力键 -> 节点数组」倒排索引。
    ///     <para>同一能力下多个节点按 WorkerId 排序，保证轮询游标的分配顺序在多次刷新之间稳定。</para>
    /// </summary>
    /// <param name="snapshot">本次刷新得到的节点集合</param>
    /// <returns>倒排索引（值为预构建数组）</returns>
    private static Dictionary<string, WorkerNodeInfo[]> BuildCapabilityIndex(Dictionary<string, WorkerNodeInfo> snapshot)
    {
        var index = new Dictionary<string, List<WorkerNodeInfo>>(StringComparer.Ordinal);
        foreach (var worker in snapshot.Values)
        {
            var capabilities = worker.Capabilities;
            if (capabilities == null) continue;

            foreach (var capability in capabilities)
            {
                if (string.IsNullOrWhiteSpace(capability)) continue;

                if (!index.TryGetValue(capability, out var list))
                {
                    list = new List<WorkerNodeInfo>();
                    index[capability] = list;
                }

                list.Add(worker);
            }
        }

        var result = new Dictionary<string, WorkerNodeInfo[]>(index.Count, StringComparer.Ordinal);
        foreach (var pair in index)
        {
            pair.Value.Sort(static (a, b) => string.CompareOrdinal(a.WorkerId, b.WorkerId));
            result[pair.Key] = pair.Value.ToArray();
        }

        return result;
    }

    /// <summary>
    ///     节点视图：节点字典 + 能力倒排索引，一次刷新整体替换。
    /// </summary>
    private sealed class WorkerView
    {
        /// <summary>
        ///     WorkerId -> 节点信息
        /// </summary>
        public ConcurrentDictionary<string, WorkerNodeInfo> Workers { get; init; } = new();

        /// <summary>
        ///     能力键（HandlerType，缺省为 JobName）-> 可执行该能力的节点数组
        /// </summary>
        public Dictionary<string, WorkerNodeInfo[]> ByCapability { get; init; } = new(StringComparer.Ordinal);
    }
}
