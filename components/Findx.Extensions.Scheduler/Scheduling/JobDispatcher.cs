using Findx.Extensions;
using Findx.Extensions.Scheduler.Models;
using Findx.Locks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     任务分发器：到期 Job -> 查可执行 Worker -> 策略选点 -> HTTP POST 分发，并登记在途执行与执行权锁。
/// </summary>
public class JobDispatcher
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WorkerRegistry _workerRegistry;
    private readonly IDispatchStrategy _dispatchStrategy;
    private readonly InFlightExecutionRegistry _inFlightRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobDispatcher> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobDispatcher(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory,
        WorkerRegistry workerRegistry, IDispatchStrategy dispatchStrategy, InFlightExecutionRegistry inFlightRegistry,
        IServiceProvider serviceProvider, ILogger<JobDispatcher> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _workerRegistry = workerRegistry;
        _dispatchStrategy = dispatchStrategy;
        _inFlightRegistry = inFlightRegistry;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     分发一次执行。返回是否成功送达 Worker。
    /// </summary>
    public async Task<bool> DispatchAsync(ScheduledJob job, string period, int attempt, CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;

        // 1. 反查该 Job 的可执行健康节点（非全部节点）
        var candidates = _workerRegistry.GetCandidates(job.JobName, job.HandlerType);
        if (candidates.Count == 0)
        {
            _logger.LogWarning("任务 [{JobName}] 无可执行 Worker 节点（JobName={JobName}, HandlerType={HandlerType}），本轮跳过",
                job.JobName, job.JobName, job.HandlerType);
            return false;
        }

        // 2. 分发策略选点（默认 RoundRobin）
        var worker = _dispatchStrategy.Select(candidates, job.JobName);
        if (worker == null)
        {
            _logger.LogWarning("任务 [{JobName}] 分发策略未选中节点，本轮跳过", job.JobName);
            return false;
        }

        // 3. 执行权锁（jobId + 周期），防同一周期重复分发/执行
        RLock execLock = null;
        var execLockKey = $"findx:scheduler:exec:{job.Id}:{period}";
        if (opts.EnableExecutionLock)
        {
            var lockTtl = TimeSpan.FromSeconds(Math.Max(60, Math.Max(opts.ExecutionTimeoutMinutes * 60, opts.MasterLockTtlSeconds * 2)));
            var lockService = _serviceProvider.GetRequiredService<ILock>("DistributedLock.Redis");
            execLock = await lockService.AcquireAsync(execLockKey, lockTtl, isWait: false, renew: true, cancellationToken);
            if (!execLock.IsLocked())
            {
                _logger.LogDebug("任务 [{JobName}] 周期 {Period} 执行权锁被占用，跳过本轮分发", job.JobName, period);
                return false;
            }
        }

        // 4. HTTP POST 分发
        var traceId = Guid.NewGuid().ToString("N");
        var request = new DispatchRequest
        {
            JobId = job.Id,
            JobName = job.JobName,
            HandlerType = job.HandlerType,
            Parameter = job.Parameter,
            TraceId = traceId,
            Period = period,
            MasterAddress = opts.NodeAddress?.TrimEnd('/')
        };

        Dictionary<string, string> headers = null;
        if (!string.IsNullOrWhiteSpace(opts.ClusterToken))
            headers = new Dictionary<string, string> { ["X-Scheduler-Token"] = opts.ClusterToken };

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{worker.Address}/api/scheduler/jobs/execute", request, headers, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("任务 [{JobName}] 分发到 {Address} 失败，HTTP {Status}", job.JobName, worker.Address, (int)response.StatusCode);
                await ReleaseExecLockAsync(execLock);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务 [{JobName}] 分发到 {Address} 异常", job.JobName, worker.Address);
            await ReleaseExecLockAsync(execLock);
            return false;
        }

        // 5. 登记在途执行（结果回报/超时兜底据此驱动重试与死信）
        _inFlightRegistry.Add(new InFlightExecution
        {
            TraceId = traceId,
            JobId = job.Id,
            JobName = job.JobName,
            HandlerType = job.HandlerType,
            Parameter = job.Parameter,
            Period = period,
            Attempt = attempt,
            WorkerAddress = worker.Address,
            StartTime = DateTime.Now,
            ExecutionLock = execLock,
            CronExpression = job.CronExpression
        });

        _logger.LogInformation("任务 [{JobName}] 第 {Attempt} 次尝试已分发至 {Worker}，TraceId={TraceId}",
            job.JobName, attempt + 1, worker.Address, traceId);
        return true;
    }

    /// <summary>
    ///     释放执行权锁
    /// </summary>
    private static async Task ReleaseExecLockAsync(RLock execLock)
    {
        if (execLock != null) await execLock.DisposeAsync();
    }
}
