using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 选举服务：抢数据表里的租约行，抢到者作为唯一调度端并周期续期。
///     <para>与 Redis 版本的区别：租约失效（时钟偏差、连接抖动）不再意味着"结果丢失"，
///     因为未完成的执行都在 <c>scheduler_execution</c> 表里，任何节点都能接管。</para>
/// </summary>
public class MasterElectionService : BackgroundService
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IMasterLease _lease;
    private readonly SchedulerJobBuffer _buffer;
    private readonly ILogger<MasterElectionService> _logger;

    private string _owner;

    /// <summary>
    ///     当前节点是否为 Master（调度循环在其它线程读取，故用 volatile 保证可见性）
    /// </summary>
    private volatile bool _isMaster;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="lease"></param>
    /// <param name="buffer"></param>
    /// <param name="logger"></param>
    public MasterElectionService(IOptions<SchedulerOptions> options, IMasterLease lease, SchedulerJobBuffer buffer,
        ILogger<MasterElectionService> logger)
    {
        _options = options;
        _lease = lease;
        _buffer = buffer;
        _logger = logger;
    }

    /// <summary>
    ///     当前节点是否为 Master
    /// </summary>
    public bool IsMaster => _isMaster;

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

        //  持有者标识带上进程号：同一台机器跑两个实例时不会互相续期对方的租约
        _owner = $"{NodeIdentity.Resolve(opts.NodeName)}#{Environment.ProcessId}";

        if (string.Equals(opts.NodeAddress?.TrimEnd('/'), "http://localhost:5000", StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("NodeAddress 为默认值 http://localhost:5000，多节点部署请显式配置 Findx:Scheduler:NodeAddress，否则分发/回调会打到本机自环");

        var ttl = TimeSpan.FromSeconds(Math.Max(5, opts.MasterLockTtlSeconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var acquired = await _lease.TryAcquireAsync(_owner, ttl, stoppingToken);
                    if (!acquired)
                    {
                        if (IsMaster)
                        {
                            _isMaster = false;
                            _logger.LogWarning("本节点 Master 租约已被其他节点接管，降级 Standby");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.MasterLockTtlSeconds / 2.0)), stoppingToken);
                        continue;
                    }

                    //  刚当选：后台预热到期窗口，避免调度循环首个周期被同步查库顶偏
                    //  （续期路径不重复预热，只在 false -> true 的跃迁上触发）
                    var promoted = !_isMaster;
                    _isMaster = true;
                    if (promoted) _ = _buffer.WarmUpAsync();

                    _logger.LogInformation("本节点 [{Node}] 持有 Master 租约，开始调度，地址 {Address}", opts.NodeName, opts.NodeAddress);

                    //  持租期：按 TTL/3 续期；续期失败即让出，避免两个节点长时间同时扫描
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.MasterLockTtlSeconds / 3.0)), stoppingToken);

                        if (!await _lease.TryAcquireAsync(_owner, ttl, stoppingToken))
                        {
                            _logger.LogWarning("Master 租约续期失败，本节点降级 Standby");
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
                    _isMaster = false;
                }
            }
        }
        finally
        {
            //  优雅停机：主动让出租约，继任者不必等 TTL 自然过期
            try
            {
                await _lease.ReleaseAsync(_owner, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "释放 Master 租约失败");
            }
        }
    }
}
