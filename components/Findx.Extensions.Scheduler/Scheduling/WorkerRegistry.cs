using Findx.Common;
using Findx.Extensions;
using Findx.Extensions.Scheduler.Models;
using Findx.Jobs.Client;
using Findx.Redis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Worker 注册表：执行端按心跳 TTL 周期上报自身能力；Master 侧周期刷新「节点缓存」并按 JobName/HandlerType 反查可执行节点。
/// </summary>
public class WorkerRegistry : BackgroundService
{
    private readonly ILogger<WorkerRegistry> _logger;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IJobFinder _jobFinder;
    private readonly IRedisClientProvider _redisClientProvider;

    private IRedisClient _redisClient;
    private string[] _localCapabilities = [];

    /// <summary>
    ///     WorkerId -> 节点信息（Master 侧缓存，整体引用替换实现原子快照，读侧无并发空窗）
    /// </summary>
    private volatile ConcurrentDictionary<string, WorkerNodeInfo> _workers = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public WorkerRegistry(IOptions<SchedulerOptions> options, IRedisClientProvider redisClientProvider,
        IJobFinder jobFinder, ILogger<WorkerRegistry> logger)
    {
        _options = options;
        _redisClientProvider = redisClientProvider;
        _jobFinder = jobFinder;
        _logger = logger;
    }

    /// <summary>
    ///     当前节点 WorkerId
    /// </summary>
    public string WorkerId => SanitizeWorkerId(_options.Value.NodeName);

    /// <summary>
    ///     主循环
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _redisClient = _redisClientProvider.CreateClient();
        var opts = _options.Value;

        if (opts.WorkerEnabled)
        {
            try
            {
                _localCapabilities = _jobFinder.FindAll(true).Where(t => t.FullName != null).Select(t => t.FullName!).ToArray();
                _logger.LogInformation("Worker 节点 {Node} 启动，注册能力 {Count} 项：{Capabilities}", opts.NodeName, _localCapabilities.Length, string.Join(",", _localCapabilities));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫描本地 IJob 能力失败");
            }

            // 部署陷阱提示：默认 NodeAddress 会导致多节点分发/回调自环
            if (string.Equals(opts.NodeAddress?.TrimEnd('/'), "http://localhost:5000", StringComparison.OrdinalIgnoreCase))
                _logger.LogWarning("NodeAddress 为默认值 http://localhost:5000，多节点部署请显式配置 Findx:Scheduler:NodeAddress，否则分发/回调会打到本机自环");
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (opts.WorkerEnabled) await HeartbeatAsync(stoppingToken);
                    await RefreshWorkerMapAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker 注册循环异常");
                }

                var interval = TimeSpan.FromSeconds(Math.Max(1, opts.WorkerHeartbeatIntervalSeconds));
                await Task.Delay(interval, stoppingToken);
            }
        }
        finally
        {
            // 下线即摘除，不必等心跳 TTL 自然过期
            if (opts.WorkerEnabled && _redisClient != null)
            {
                try
                {
                    await _redisClient.HDelAsync(opts.WorkerIndexKey, WorkerId, CancellationToken.None);
                    _logger.LogInformation("Worker 节点 {Node} 已下线并摘除注册", opts.NodeName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Worker 下线摘除注册失败");
                }
            }
        }
    }

    /// <summary>
    ///     心跳上报：Hash 注册表覆盖写 field=WorkerId（field 级 TTL 由刷新侧按 HeartbeatTime 判定，过期自动摘除）
    /// </summary>
    private async Task HeartbeatAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        var info = new WorkerNodeInfo
        {
            WorkerId = WorkerId,
            Name = opts.NodeName,
            Address = opts.NodeAddress?.TrimEnd('/'),
            Weight = 1,
            Capabilities = _localCapabilities.ToList(),
            HeartbeatTime = DateTime.Now
        };

        await _redisClient.HSetAsync(opts.WorkerIndexKey, WorkerId, info.ToJson(), When.Always, cancellationToken);
    }

    /// <summary>
    ///     Master 侧刷新节点缓存：HGetAll 一次拉全量（避免 Redis KEYS 扫描），按 HeartbeatTime 判活，
    ///     过期 field 顺手 HDEL 摘除；整体引用替换实现原子快照，读侧无空窗
    /// </summary>
    private async Task RefreshWorkerMapAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        var all = await _redisClient.HGetAllAsync(opts.WorkerIndexKey, cancellationToken);
        var alive = new Dictionary<string, WorkerNodeInfo>();
        var dead = new List<string>();
        var ttl = TimeSpan.FromSeconds(Math.Max(1, opts.WorkerHeartbeatTtlSeconds));

        foreach (var kv in all)
        {
            try
            {
                var info = kv.Value.ToObject<WorkerNodeInfo>();
                if (info == null || string.IsNullOrWhiteSpace(info.WorkerId))
                {
                    dead.Add(kv.Key);
                    continue;
                }

                // 心跳超过 TTL 视为下线：缓存剔除 + 注册表摘除
                if (DateTime.Now - info.HeartbeatTime > ttl)
                {
                    dead.Add(kv.Key);
                    continue;
                }

                alive[info.WorkerId] = info;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "解析 Worker 注册 {Key} 失败", kv.Key);
                dead.Add(kv.Key);
            }
        }

        foreach (var id in dead)
        {
            try
            {
                await _redisClient.HDelAsync(opts.WorkerIndexKey, id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "摘除过期 Worker 注册 {Key} 失败", id);
            }
        }

        // 原子替换：GetCandidates 读取的是替换前的完整快照，不会读到半填充状态
        _workers = new ConcurrentDictionary<string, WorkerNodeInfo>(alive);
    }

    /// <summary>
    ///     按 JobName / HandlerType 反查可执行的健康 Worker 列表
    /// </summary>
    public IReadOnlyList<WorkerNodeInfo> GetCandidates(string jobName, string handlerType)
    {
        var workers = _workers; // 局部快照：整体替换语义下读到完整旧快照或完整新快照
        return workers.Values
            .Where(w => w.Capabilities != null &&
                        w.Capabilities.Any(c =>
                            string.Equals(c, jobName, StringComparison.Ordinal) ||
                            string.Equals(c, handlerType, StringComparison.Ordinal)))
            .ToList();
    }

    /// <summary>
    ///     WorkerId 规范化：去除 key 非法字符
    /// </summary>
    private static string SanitizeWorkerId(string nodeName)
    {
        var id = string.IsNullOrWhiteSpace(nodeName) ? Environment.MachineName : nodeName.Trim();
        foreach (var ch in new[] { ' ', ':', '*', '?', '\n', '\r', '\t' }) id = id.Replace(ch, '-');
        return id;
    }
}
