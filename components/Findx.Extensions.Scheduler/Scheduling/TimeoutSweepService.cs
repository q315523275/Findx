using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     执行账本超时兜底服务（只在 Master 上跑，独立节拍）。
///     <para>从调度循环里拆出来的原因：兜底是「两趟表扫描 + 逐行终结/失败处理」的批处理，
///     一次集中超时（例如整个执行端集群重启）会产生 200 次以上的写操作；留在 5 秒调度循环里，
///     单轮耗时一旦超过扫描间隔，秒级 cron 就会出现明显漂移。</para>
///     <para>语义不变：Claimed 超过领取确认时限按失败重试；Dispatched 超过执行超时按失败重试或死信隔离；
///     两类都先抢终结权（TryFinalize），抢不到说明回调已到，直接跳过。</para>
/// </summary>
public class TimeoutSweepService : BackgroundService
{
    /// <summary>
    ///     每处理多少行让出一次（配合 <see cref="SchedulerOptions.SweepBatchDelayMs" />）
    /// </summary>
    private const int YieldEveryRows = 25;

    private readonly IOptions<SchedulerOptions> _options;
    private readonly MasterElectionService _electionService;
    private readonly IExecutionLedger _ledger;
    private readonly JobResultHandler _resultHandler;
    private readonly IClusterClock _clock;
    private readonly ILogger<TimeoutSweepService> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="electionService"></param>
    /// <param name="ledger"></param>
    /// <param name="resultHandler"></param>
    /// <param name="clock"></param>
    /// <param name="logger"></param>
    public TimeoutSweepService(IOptions<SchedulerOptions> options, MasterElectionService electionService,
        IExecutionLedger ledger, JobResultHandler resultHandler, IClusterClock clock,
        ILogger<TimeoutSweepService> logger)
    {
        _options = options;
        _electionService = electionService;
        _ledger = ledger;
        _resultHandler = resultHandler;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    ///     主循环：独立节拍，与到期扫描互不影响
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;
        if (!opts.SchedulerEnabled) return;

        var intervalMs = Math.Max(5, opts.SweepIntervalSeconds) * 1000;

        //  启动后先等一个间隔：避开进程启动瞬间的建表/预热流量
        await Task.Delay(TimeSpan.FromMilliseconds(intervalMs), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.Now;
            try
            {
                //  Standby 不写账本：多节点同时兜底会把同一行反复终结（幂等但浪费连接）
                if (_electionService.IsMaster) await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "超时兜底循环异常");
            }

            var costMs = (DateTime.Now - startTime).TotalMilliseconds;
            var delayMs = intervalMs - costMs;
            if (delayMs > 0) await Task.Delay(TimeSpan.FromMilliseconds(delayMs), stoppingToken);
        }
    }

    /// <summary>
    ///     一次兜底：先扫未确认受理，再扫已受理无回调
    /// </summary>
    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        var now = _clock.Now;
        var batch = Math.Max(1, opts.SweepBatchSize);

        //  1. Claimed 超时：领取后进程崩溃 / 投递未送达
        var claimTimeoutSeconds = Math.Max(5, opts.ClaimTimeoutSeconds);
        var claimedRows = await _ledger.ListStaleAsync(now.AddSeconds(-claimTimeoutSeconds), batch, cancellationToken);
        await FinalizeBatchAsync(claimedRows, ExecutionState.Claimed,
            "投递确认超时（执行节点未确认受理）", opts, cancellationToken);

        //  2. Dispatched 超时：Worker 未回调
        if (opts.ExecutionTimeoutMinutes <= 0) return;

        var dispatchedRows = await _ledger.ListStaleAsync(now.AddMinutes(-opts.ExecutionTimeoutMinutes), batch, cancellationToken);
        await FinalizeBatchAsync(dispatchedRows, ExecutionState.Dispatched,
            $"执行超时：{opts.ExecutionTimeoutMinutes} 分钟无结果回调", opts, cancellationToken);
    }

    /// <summary>
    ///     批量终结：逐行抢终结权，抢到后按失败处理（退避重试或死信隔离）
    /// </summary>
    private async Task FinalizeBatchAsync(IReadOnlyList<SchedulerExecution> rows, ExecutionState expectedState,
        string reason, SchedulerOptions opts, CancellationToken cancellationToken)
    {
        if (rows == null || rows.Count == 0) return;

        var handled = 0;
        var delayMs = Math.Max(0, opts.SweepBatchDelayMs);

        foreach (var execution in rows)
        {
            if (cancellationToken.IsCancellationRequested) break;

            //  查询与终结之间状态可能已被回调推进，按预期状态过滤
            if (execution.State != expectedState) continue;
            if (!await _ledger.TryFinalizeAsync(execution.TraceId, ExecutionState.Failed, reason, _clock.Now, null, cancellationToken))
                continue;

            _logger.LogWarning("任务 [{JobName}] TraceId={TraceId} {Reason}，按失败处理", execution.JobName, execution.TraceId, reason);
            await _resultHandler.HandleFailureAsync(execution, reason, cancellationToken);

            handled++;
            //  让出：批量写库时保持连接池与线程调度可用，避免一次兜底把整段时间占满
            if (delayMs > 0 && handled % YieldEveryRows == 0) await Task.Delay(delayMs, cancellationToken);
        }

        if (handled > 0) _logger.LogInformation("超时兜底完成一批：状态 {State}，处理 {Count} 行", expectedState, handled);
    }
}
