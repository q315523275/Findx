using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     Worker 端执行受理队列：接口收到分发请求后仅入队并立即返回受理成功，
///     实际执行与结果回调都在独立消费线程完成。
///     <para>好处：Master 侧分发等待时长不再等于任务执行时长，长任务不会触发在途超时导致重复执行。</para>
///     <para>代价：受理后进程崩溃会丢失该次触发，依赖 Master 侧在途超时兜底转为重试（至少一次语义）。</para>
/// </summary>
public class JobExecutionQueue : IHostedService, IDisposable
{
    private readonly Channel<DispatchRequest> _channel;
    private readonly List<Task> _consumers = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly JobResultReporter _reporter;
    private readonly ILogger<JobExecutionQueue> _logger;
    private readonly int _workerCount;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobExecutionQueue(IOptions<SchedulerOptions> options, IServiceScopeFactory scopeFactory,
        JobResultReporter reporter, ILogger<JobExecutionQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _reporter = reporter;
        _logger = logger;
        _workerCount = Math.Max(1, options.Value.ExecutionWorkerCount);

        var capacity = Math.Max(64, options.Value.ExecutionQueueCapacity);
        //  有界通道：积压超容量即拒绝受理，让 Master 走重试，而不是把请求堆在内存或拖住 HTTP 线程
        _channel = Channel.CreateBounded<DispatchRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    /// <summary>
    ///     尝试受理一个执行请求。返回 false 表示队列已满（调用方应返回失败让 Master 重试）
    /// </summary>
    public bool TryEnqueue(DispatchRequest request) => _channel.Writer.TryWrite(request);

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
    ///     消费循环：执行 IJob 并把结果回调 Master
    /// </summary>
    private async Task ConsumeAsync(ChannelReader<DispatchRequest> reader, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var request in reader.ReadAllAsync(cancellationToken))
            {
                var watch = Stopwatch.StartNew();
                bool success;
                string error = null;
                string message = null;

                try
                {
                    //  每次执行独立 DI 作用域：IJob 及其依赖按 Scoped 释放，长跑进程不累积实例
                    using var scope = _scopeFactory.CreateScope();
                    var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
                    var jobExecuteInfo = new JobExecuteInfo
                    {
                        Id = SnowflakeIdUtility.Default().NextId(),
                        JobId = request.JobId,
                        Name = request.JobName,
                        Parameter = request.Parameter,
                        FullName = request.HandlerType,
                        RunTime = DateTimeOffset.Now
                    };

                    var context = new JobExecuteContext(scope.ServiceProvider, jobExecuteInfo);
                    await executor.ExecuteAsync(context, cancellationToken);
                    success = context.JobResult.IsSuccess && !context.JobResult.IsCancelled;
                    error = context.JobResult.Error?.Message;
                    message = context.JobResult.Message;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    success = false;
                    error = ex.Message;
                    message = ex.ToString();
                }

                watch.Stop();

                //  回调在消费线程发出：HTTP 请求线程已返回，不再等待任务执行
                try
                {
                    await _reporter.ReportAsync(request, success, error, message, watch.ElapsedMilliseconds, cancellationToken);
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
