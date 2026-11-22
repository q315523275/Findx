using System.Diagnostics;
using System.Threading.Channels;
using Findx.DependencyInjection;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.NewId;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     Worker 端执行受理队列：接口收到分发请求后仅入队并立即返回受理成功，
///     实际执行与结果回调都在独立消费线程完成。
///     <para>好处：Master 侧分发等待时长不再等于任务执行时长，长任务不会触发在途超时导致重复执行。</para>
///     <para>代价：受理后进程崩溃会丢失该次触发，依赖调度端执行账本的超时兜底转为重试（至少一次语义）。</para>
/// </summary>
public class JobExecutionQueue : IHostedService, IDisposable
{
    private readonly Channel<DispatchRequest> _channel;
    private readonly List<Task> _consumers = [];
    private readonly CancellationTokenSource _cts = new();
    private readonly JobResultReporter _reporter;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly ExecutionLogCollector _logCollector;
    private readonly ExecutionLogShipper _logShipper;
    private readonly ILogger<JobExecutionQueue> _logger;
    private readonly int _workerCount;
    private readonly int _maxLogChars;

    /// <summary>
    ///     运行中的执行（TraceId -> 取消令牌），供人工 kill 使用
    /// </summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobExecutionQueue(IOptions<SchedulerOptions> options, JobResultReporter reporter,
        IKeyGenerator<long> keyGenerator, ExecutionLogCollector logCollector, ExecutionLogShipper logShipper,
        ILogger<JobExecutionQueue> logger)
    {
        _reporter = reporter;
        _keyGenerator = keyGenerator;
        _logCollector = logCollector;
        _logShipper = logShipper;
        _logger = logger;
        _workerCount = options.Value.EffectiveExecutionWorkerCount;
        _maxLogChars = options.Value.MaxExecutionLogChars;

        var capacity = Math.Max(64, options.Value.EffectiveExecutionQueueCapacity);
        //  有界通道：积压超容量即拒绝受理，让调度端走重试，而不是把请求堆在内存或拖住 HTTP 线程
        _channel = Channel.CreateBounded<DispatchRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    /// <summary>
    ///     尝试受理一个执行请求。返回 false 表示队列已满（调用方应返回失败让调度端重试）
    /// </summary>
    public bool TryEnqueue(DispatchRequest request) => _channel.Writer.TryWrite(request);

    /// <summary>
    ///     人工终止一个正在执行的 Job（按 TraceId 取消其执行令牌）
    /// </summary>
    /// <param name="traceId"></param>
    /// <returns>false=该 TraceId 不在本节点执行中（可能已结束或投递到别的节点）</returns>
    public bool Kill(string traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId)) return false;

        if (!_running.TryGetValue(traceId, out var cts)) return false;

        cts.Cancel();
        _logger.LogWarning("TraceId={TraceId} 收到人工终止请求，已取消执行", traceId);
        return true;
    }

    /// <summary>
    ///     启动执行消费线程
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        _consumers.Clear();
        for (var i = 0; i < _workerCount; i++)
        {
            _consumers.Add(Task.Run(() => ConsumeAsync(reader, _cts.Token), CancellationToken.None));
        }

        _logger.LogInformation("任务执行受理队列已启动，消费线程数={Count}", _workerCount);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止消费线程
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        try
        {
            await Task.WhenAll(_consumers);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "等待任务执行消费线程退出时发生异常");
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        try
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
        }
        catch (ObjectDisposedException)
        {
            //  已释放或已停止，忽略
        }
        _cts.Dispose();
    }

    /// <summary>
    ///     消费循环：执行 IJob 并把结果回调调度端
    /// </summary>
    private async Task ConsumeAsync(ChannelReader<DispatchRequest> reader, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var request in reader.ReadAllAsync(cancellationToken))
            {
                //  执行期日志按 TraceId 旁路采集：Job 及其依赖的 ILogger 输出都会进入本次执行记录；
                //  达到分片阈值即交给上报队列，长任务的日志不必等执行结束才可见
                using var logScope = _logCollector.Begin(request.TraceId, request.JobId, chunk =>
                {
                    chunk.MasterAddress = request.MasterAddress;
                    _logShipper.Ship(chunk);
                });
                var parameterPreview = request.Parameter?.Length > 200 ? request.Parameter[..200] + "…" : request.Parameter;
                logScope.Write($"开始执行 Job={request.JobName} TraceId={request.TraceId} Period={request.Period} 参数={parameterPreview}");

                var watch = Stopwatch.StartNew();
                bool success = false;
                string error = null;
                string message = null;
                var killed = false;

                //  每个执行独立可取消令牌：人工 kill 只影响这一条，不影响其他执行
                var executionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _running[request.TraceId] = executionCts;

                try
                {
                    //  每次执行独立 DI 作用域：IJob 及其依赖按 Scoped 释放，长跑进程不累积实例
                    await using var scope = ServiceLocator.CreateAsyncScope();
                    var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
                    var jobExecuteInfo = new JobExecuteInfo
                    {
                        Id = _keyGenerator.Create(),
                        JobId = request.JobId,
                        Name = request.JobName,
                        Parameter = request.Parameter,
                        FullName = request.HandlerType,
                        RunTime = DateTimeOffset.Now
                    };

                    var context = new JobExecuteContext(scope.ServiceProvider, jobExecuteInfo)
                    {
                        //  执行上下文暴露日志记录能力，Job 可主动记录过程信息（与 ILogger 采集互补）
                        LogRecorder = logScope
                    };
                    await executor.ExecuteAsync(context, executionCts.Token);
                    success = context.JobResult.IsSuccess && !context.JobResult.IsCancelled;
                    error = context.JobResult.Error?.Message;
                    message = context.JobResult.Message;

                    logScope.Write(success
                        ? $"执行完成 成功，耗时 {watch.ElapsedMilliseconds}ms"
                        : $"执行完成 失败，耗时 {watch.ElapsedMilliseconds}ms：{error ?? message}");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    //  宿主停机：不入结果，交给调度端超时兜底
                    break;
                }
                catch (OperationCanceledException) when (executionCts.IsCancellationRequested)
                {
                    killed = true;
                    success = false;
                    error = "execution killed by operator";
                    message = "任务被人工终止";
                    logScope.Write($"执行被人工终止，耗时 {watch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    success = false;
                    error = ex.Message;
                    message = ex.ToString();
                    logScope.Write($"执行抛出异常，耗时 {watch.ElapsedMilliseconds}ms：{ex.GetType().Name}: {ex.Message}");
                }
                finally
                {
                    _running.TryRemove(request.TraceId, out _);
                    executionCts.Dispose();
                }

                watch.Stop();

                if (killed)
                {
                    _logger.LogWarning("任务 [{JobName}] TraceId={TraceId} 已被人工终止", request.JobName, request.TraceId);
                    continue;
                }

                //  先补发最后一片日志明细，再把尾部摘要随结果回调带回（明细全量、回调轻量）
                logScope.FlushFinal();
                var logs = logScope.Render(_maxLogChars);
                try
                {
                    await _reporter.ReportAsync(request, success, error, message, watch.ElapsedMilliseconds, logs, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "任务 [{JobName}] TraceId={TraceId} 结果上报异常", request.JobName, request.TraceId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            //  停止时正常退出
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务执行消费循环异常退出");
        }
    }
}
