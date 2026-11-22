using Findx.Extensions;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.NewId;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     基于数据表的 Worker 节点目录。
///     <para>执行端通过 HTTP 心跳上报，调度端（本类）负责落库、判活与摘除；
///     两侧读写的都是同一张 <c>scheduler_worker</c> 表，因此不再需要 Redis Hash 注册表。</para>
/// </summary>
public class DatabaseWorkerDirectory : IWorkerDirectory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IClusterClock _clock;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly ILogger<DatabaseWorkerDirectory> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DatabaseWorkerDirectory(IServiceProvider serviceProvider, IClusterClock clock,
        IKeyGenerator<long> keyGenerator, ILogger<DatabaseWorkerDirectory> logger)
    {
        _serviceProvider = serviceProvider;
        _clock = clock;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     写入/刷新心跳（按 WorkerId 幂等覆盖）
    /// </summary>
    public async Task HeartbeatAsync(WorkerNodeInfo worker, CancellationToken cancellationToken = default)
    {
        if (worker == null || string.IsNullOrWhiteSpace(worker.WorkerId)) return;

        var capabilities = (worker.Capabilities ?? new List<string>()).ToJson();
        var at = _clock.Now;

        var exists = await DbAccess.UseAsync<SchedulerWorker, bool>(_serviceProvider,
            repo => repo.ExistAsync(x => x.WorkerId == worker.WorkerId, cancellationToken), cancellationToken);

        if (!exists)
        {
            try
            {
                await DbAccess.UseAsync<SchedulerWorker, int>(_serviceProvider,
                    repo => repo.InsertAsync(new SchedulerWorker
                    {
                        //  多节点注册必须显式主键：该列无 AUTO_INCREMENT，所有节点都会写 Id=0
                        Id = _keyGenerator.Create(),
                        WorkerId = worker.WorkerId,
                        Name = worker.Name,
                        Address = worker.Address,
                        Weight = worker.Weight <= 0 ? 1 : worker.Weight,
                        Capabilities = capabilities,
                        HeartbeatAt = at
                    }, cancellationToken), cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                //  并发注册：唯一键冲突说明别的请求已插入，转为覆盖写
                _logger.LogDebug(ex, "Worker {WorkerId} 注册行并发创建，转为覆盖写", worker.WorkerId);
            }
        }

        await DbAccess.UseAsync<SchedulerWorker, int>(_serviceProvider,
            repo => repo.UpdateColumnsAsync(
                x => new SchedulerWorker
                {
                    Name = worker.Name,
                    Address = worker.Address,
                    Weight = worker.Weight <= 0 ? 1 : worker.Weight,
                    Capabilities = capabilities,
                    HeartbeatAt = at
                },
                x => x.WorkerId == worker.WorkerId,
                cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     摘除节点
    /// </summary>
    public async Task RemoveAsync(string workerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workerId)) return;

        await DbAccess.UseAsync<SchedulerWorker, int>(_serviceProvider,
            repo => repo.DeleteAsync(x => x.WorkerId == workerId, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     查询存活节点
    /// </summary>
    public async Task<IReadOnlyList<WorkerNodeInfo>> ListAliveAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var rows = await DbAccess.UseAsync<SchedulerWorker, List<SchedulerWorker>>(_serviceProvider,
            repo => repo.SelectAsync(x => x.HeartbeatAt >= since, cancellationToken: cancellationToken), cancellationToken);

        var result = new List<WorkerNodeInfo>(rows.Count);
        foreach (var row in rows)
        {
            result.Add(new WorkerNodeInfo
            {
                WorkerId = row.WorkerId,
                Name = row.Name,
                Address = row.Address,
                Weight = row.Weight,
                Capabilities = ParseCapabilities(row.Capabilities),
                HeartbeatTime = row.HeartbeatAt
            });
        }

        return result;
    }

    /// <summary>
    ///     清理过期节点记录
    /// </summary>
    public async Task<int> PruneAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        return await DbAccess.UseAsync<SchedulerWorker, int>(_serviceProvider,
            repo => repo.DeleteAsync(x => x.HeartbeatAt < before, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     能力清单反序列化（脏数据降级为空清单，不影响调度循环）
    /// </summary>
    private List<string> ParseCapabilities(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return json.ToObject<List<string>>() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Worker 能力清单解析失败：{Json}", json);
            return new List<string>();
        }
    }
}
