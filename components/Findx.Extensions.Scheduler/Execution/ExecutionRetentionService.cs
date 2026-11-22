using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Scheduling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行历史保留策略：按 `RetentionDays` 清理已终结的执行账本与对应日志明细。
///     <para>只删"已终结 + FinishAt 早于截止时间"的行；日志明细与账本行按同一批 TraceId 一起删，
///     避免出现"账本已删、明细留成孤儿"或"明细先删、账本还在但日志读不到"。</para>
///     <para>仅在 Master 上执行（租约持有者），多节点同时跑也只会重复删除已删行（影响行数 0），不会误删。</para>
/// </summary>
public sealed class ExecutionRetentionService : IHostedService, IDisposable
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IExecutionLedger _ledger;
    private readonly IExecutionLogStore _logStore;
    private readonly MasterElectionService _election;
    private readonly ILogger<ExecutionRetentionService> _logger;
    private readonly CancellationTokenSource _cts = new();

    private Task _task;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ExecutionRetentionService(IOptions<SchedulerOptions> options, IExecutionLedger ledger,
        IExecutionLogStore logStore, MasterElectionService election, ILogger<ExecutionRetentionService> logger)
    {
        _options = options;
        _ledger = ledger;
        _logStore = logStore;
        _election = election;
        _logger = logger;
    }

    /// <summary>
    ///     启动清理循环
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.RetentionDays <= 0)
        {
            _logger.LogInformation("执行历史清理未启用（RetentionDays<=0）");
            return Task.CompletedTask;
        }

        _task = Task.Run(() => LoopAsync(_cts.Token), CancellationToken.None);
        _logger.LogInformation("执行历史清理已启动：保留 {Days} 天，扫描间隔 {Interval} 分钟",
            _options.Value.RetentionDays, _options.Value.RetentionSweepIntervalMinutes);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        if (_task != null)
        {
            try
            {
                await Task.WhenAny(_task, Task.Delay(TimeSpan.FromSeconds(3), cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "执行历史清理停止时异常");
            }
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <summary>
    ///     清理循环：启动后先等 1 分钟（避开启动期），之后按配置间隔扫描
    /// </summary>
    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(10, _options.Value.RetentionSweepIntervalMinutes));

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_election.IsMaster) await SweepAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行历史清理失败，等待下一轮");
                }

                await Task.Delay(interval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //  正常停止
        }
    }

    /// <summary>
    ///     单轮清理：分批删除，单轮最多 200 批（防止一次性删爆数据库）
    /// </summary>
    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.Now.AddDays(-_options.Value.RetentionDays);
        var batchSize = Math.Clamp(_options.Value.RetentionBatchSize, 20, 2000);

        var executions = 0;
        var logs = 0;

        for (var batch = 0; batch < 200; batch++)
        {
            var rows = await _ledger.ListTerminalBeforeAsync(cutoff, batchSize, cancellationToken);
            if (rows.Count == 0) break;

            var traceIds = rows.Select(x => x.TraceId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
            if (traceIds.Length == 0) break;

            //  先删明细再删账本：反过来的话，中途失败会留下"账本没了但有日志孤儿"
            logs += await _logStore.DeleteByTraceIdsAsync(traceIds, cancellationToken);
            executions += await _ledger.DeleteByTraceIdsAsync(traceIds, cancellationToken);

            if (rows.Count < batchSize) break;
        }

        if (executions > 0 || logs > 0)
        {
            _logger.LogInformation("执行历史清理完成：截止 {Cutoff:yyyy-MM-dd HH:mm}，删除执行 {Executions} 行、日志 {Logs} 片",
                cutoff, executions, logs);
        }
    }
}
