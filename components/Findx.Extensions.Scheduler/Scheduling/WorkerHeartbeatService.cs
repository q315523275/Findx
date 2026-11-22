using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Jobs.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     执行端心跳服务（只存在于执行端进程）。
///     <para>职责：扫描本地 IJob 清单 → 通过 <see cref="ISchedulerMasterChannel" /> 上报调度端 → 停机时下线。
///     不注入任何数据库原语：节点登记、能力清单的落库全部由调度端接口完成，执行端不需要写库权限。</para>
/// </summary>
public class WorkerHeartbeatService : BackgroundService
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IClusterClock _clock;
    private readonly IJobFinder _jobFinder;
    private readonly ISchedulerMasterChannel _channel;
    private readonly ILogger<WorkerHeartbeatService> _logger;

    private volatile string[] _localCapabilities = [];
    private bool _endpointWarned;
    private DateTime _capabilitiesRefreshAt = DateTime.MinValue;
    private readonly object _capabilityLock = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public WorkerHeartbeatService(IOptions<SchedulerOptions> options, IClusterClock clock, IJobFinder jobFinder,
        ISchedulerMasterChannel channel, ILogger<WorkerHeartbeatService> logger)
    {
        _options = options;
        _clock = clock;
        _jobFinder = jobFinder;
        _channel = channel;
        _logger = logger;
    }

    /// <summary>
    ///     当前节点 WorkerId
    /// </summary>
    public string WorkerId => NodeIdentity.Resolve(_options.Value.NodeName);

    /// <summary>
    ///     本节点当前上报的任务清单快照
    /// </summary>
    public IReadOnlyList<string> LocalCapabilities => _localCapabilities;

    /// <summary>
    ///     主循环
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;

        RefreshLocalCapabilities(true);

        if (string.Equals(opts.NodeAddress?.TrimEnd('/'), "http://localhost:5000", StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("NodeAddress 为默认值 http://localhost:5000，多节点部署请显式配置 Findx:Scheduler:NodeAddress，否则分发/回调会打到本机自环");

        if ((opts.MasterEndpoints ?? Array.Empty<string>()).Length == 0 && !opts.SchedulerEnabled && !_endpointWarned)
        {
            _endpointWarned = true;
            _logger.LogWarning("Findx:Scheduler:MasterEndpoints 未配置且本机未启用调度端，Worker 无法上报心跳，调度端将看不到本节点");
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //  清单可能在运行期变化（插件热加载、条件注册），按间隔刷新后再上报
                    RefreshLocalCapabilities(false);
                    await _channel.HeartbeatAsync(BuildLocalInfo(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker 心跳循环异常");
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.WorkerHeartbeatIntervalSeconds)), stoppingToken);
            }
        }
        finally
        {
            //  停机尽力摘除：失败不影响进程退出，调度端靠 TTL 兜底清理
            await _channel.OfflineAsync(new WorkerNodeInfo { WorkerId = WorkerId, Name = opts.NodeName });
            _logger.LogInformation("Worker 节点 {Node} 已发送下线通知（协议 {Protocol}）", opts.NodeName, _channel.Protocol);
        }
    }

    /// <summary>
    ///     重新扫描本地 IJob 实现，生成该节点的任务清单。
    ///     <para>清单有变化时记 Info：节点能力变化会直接改变分发路由，运维需要能追溯。</para>
    /// </summary>
    private void RefreshLocalCapabilities(bool force)
    {
        var interval = Math.Max(10, _options.Value.CapabilityRefreshSeconds);
        if (!force && (_clock.Now - _capabilitiesRefreshAt).TotalSeconds < interval) return;

        lock (_capabilityLock)
        {
            if (!force && (_clock.Now - _capabilitiesRefreshAt).TotalSeconds < interval) return;

            try
            {
                var types = _jobFinder.FindAll(true)
                    .Where(t => t.FullName != null)
                    .Select(t => t.FullName!)
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                var previous = _localCapabilities;
                _localCapabilities = types;
                _capabilitiesRefreshAt = _clock.Now;

                if (!force && types.Length == previous.Length && !types.Except(previous).Any()) return;

                _logger.LogInformation("Worker 节点 {Node} 任务清单 {Action}，共 {Count} 项：{Capabilities}",
                    _options.Value.NodeName, force ? "已加载" : "已刷新", types.Length, string.Join(",", types));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫描本地 IJob 任务清单失败，沿用上一次清单（{Count} 项）", _localCapabilities.Length);
            }
        }
    }

    /// <summary>
    ///     本节点自身信息（地址取 NodeAddress，能力取本地扫描结果）
    /// </summary>
    private WorkerNodeInfo BuildLocalInfo()
    {
        var opts = _options.Value;
        return new WorkerNodeInfo
        {
            WorkerId = WorkerId,
            Name = opts.NodeName,
            Address = opts.NodeAddress?.TrimEnd('/'),
            Weight = 1,
            Capabilities = _localCapabilities.ToList(),
            HeartbeatTime = _clock.Now
        };
    }
}
