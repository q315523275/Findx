using Findx.Common;
using Findx.Extensions;
using Findx.Extensions.Scheduler.Options;
using Findx.Locks;
using Findx.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 选举服务：多节点启动即抢 Redis 锁，抢到者唯一 Master 并自动续期；
///     锁过期/释放后其他节点自动重新竞争，新 Master 从持久化状态续跑；非 Master 为 Standby。
/// </summary>
public class MasterElectionService : BackgroundService
{
    /// <summary>
    ///     Redis 实际 key 前缀（来自 Findx.Locks.LockBase.GenerateNewLockKey，用于校验持锁归属）
    /// </summary>
    private const string LockKeyPrefix = "lock:";

    private readonly ILogger<MasterElectionService> _logger;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    private ILock _lock;
    private IRedisClient _redisClient;

    /// <summary>
    ///     Ctor
    /// </summary>
    public MasterElectionService(IOptions<SchedulerOptions> options, IServiceProvider serviceProvider, ILogger<MasterElectionService> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     当前节点是否为 Master
    /// </summary>
    public bool IsMaster { get; private set; }

    /// <summary>
    ///     主循环
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;
        if (!opts.SchedulerEnabled)
        {
            _logger.LogInformation("本节点 SchedulerEnabled=false，不参与 Master 选举");
            return;
        }

        _lock = _serviceProvider.GetRequiredService<ILock>("DistributedLock.Redis");
        _redisClient = _serviceProvider.GetRequiredService<IRedisClientProvider>().CreateClient();

        // 部署陷阱提示：默认 NodeAddress 会导致分发/回调自环
        if (string.Equals(opts.NodeAddress?.TrimEnd('/'), "http://localhost:5000", StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("NodeAddress 为默认值 http://localhost:5000，多节点部署请显式配置 Findx:Scheduler:NodeAddress，否则分发/回调会打到本机自环");

        while (!stoppingToken.IsCancellationRequested)
        {
            RLock rLock = null;
            try
            {
                // 非阻塞抢锁；renew=true 时内部 AsyncTimer 按 TTL/3 自动 Lua 原子续期
                rLock = await _lock.AcquireAsync(opts.MasterLockKey,
                    TimeSpan.FromSeconds(opts.MasterLockTtlSeconds), isWait: false, renew: true, stoppingToken);

                if (!rLock.IsLocked())
                {
                    if (IsMaster)
                    {
                        IsMaster = false;
                        _logger.LogWarning("本节点 Master 锁未抢回，降级 Standby");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.MasterLockTtlSeconds / 2.0)), stoppingToken);
                    continue;
                }

                // 抢到锁：成为唯一 Master
                IsMaster = true;
                _logger.LogInformation("本节点 [{Node}] 抢到 Master 锁，成为 Master，调度地址 {Address}", opts.NodeName, opts.NodeAddress);
                await WriteMasterInfoAsync(rLock, stoppingToken);

                // 持锁期：周期续约 MasterInfo 并校验持锁归属（脑裂/分区检测）
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.MasterLockTtlSeconds / 3.0)), stoppingToken);
                    await WriteMasterInfoAsync(rLock, stoppingToken);

                    var currentValue = await _redisClient.StringGetAsync(LockKeyPrefix + opts.MasterLockKey, stoppingToken);
                    if (!string.Equals(currentValue, rLock.LockId, StringComparison.Ordinal))
                    {
                        _logger.LogWarning("Master 锁已易主（疑似网络分区/GC 停顿），本节点降级 Standby");
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Master 选举循环异常，5 秒后重试");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            finally
            {
                IsMaster = false;
                if (rLock != null) await rLock.DisposeAsync();
            }
        }
    }

    /// <summary>
    ///     把本节点地址写入 MasterInfo key，供 Worker 回调；TTL 约为锁租期 3 倍，随持锁周期续约。
    ///     仅持锁者允许写——双 Master 窗口内旧主不得覆盖新主地址，避免 Worker 回调错节点
    /// </summary>
    /// <param name="rLock"></param>
    /// <param name="cancellationToken"></param>
    private async Task WriteMasterInfoAsync(RLock rLock, CancellationToken cancellationToken)
    {
        //  写前校验持锁归属：锁已失效（易主/续期失败）则不再更新 MasterInfo
        if (rLock == null || !rLock.IsLocked()) return;

        var opts = _options.Value;
        var info = new
        {
            Address = opts.NodeAddress?.TrimEnd('/'),
            opts.NodeName,
            StartedAt = DateTime.Now
        };

        await _redisClient.StringSetAsync(opts.MasterInfoKey, info.ToJson(), TimeSpan.FromSeconds(Math.Max(3, opts.MasterLockTtlSeconds * 3)), When.Always, cancellationToken);
    }
}
