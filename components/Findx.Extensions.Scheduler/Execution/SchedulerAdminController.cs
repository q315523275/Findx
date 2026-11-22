using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Extensions.Scheduler.Storage;
using Findx.Jobs.Common;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     调度管理面：任务定义维护、手动触发、执行记录查询、人工终止、节点与主节点状态。
///     <para>机器间接口，需携带 X-Scheduler-Token；用户级鉴权交由宿主网关/登录态处理。</para>
/// </summary>
[Area("findx")]
[Route("api/scheduler/admin")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度管理")]
[MasterRoleOnly]
public class SchedulerAdminController : AreaApiControllerBase
{
    /// <summary>
    ///     执行记录单次查询上限
    /// </summary>
    private const int MaxExecutionPageSize = 200;

    private readonly IOptions<SchedulerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    //  管理面只在调度端可用：依赖项全部惰性解析，纯执行端启动时不触发任何数据库原语的构造
    private ScheduledJobStorage _storage => LazyServices.Require<ScheduledJobStorage>(_serviceProvider);
    private IPeriodClaimer _claimer => LazyServices.Require<IPeriodClaimer>(_serviceProvider);
    private JobDispatcher _dispatcher => LazyServices.Require<JobDispatcher>(_serviceProvider);
    private IExecutionLedger _ledger => LazyServices.Require<IExecutionLedger>(_serviceProvider);
    private IExecutionLogStore _logStore => LazyServices.Require<IExecutionLogStore>(_serviceProvider);
    private IWorkerDirectory _directory => LazyServices.Require<IWorkerDirectory>(_serviceProvider);
    private IMasterLease _lease => LazyServices.Require<IMasterLease>(_serviceProvider);
    private WorkerRegistry _workerRegistry => LazyServices.Require<WorkerRegistry>(_serviceProvider);
    private IClusterClock _clock => LazyServices.Require<IClusterClock>(_serviceProvider);
    private IHttpClientFactory _httpClientFactory => LazyServices.Require<IHttpClientFactory>(_serviceProvider);

    /// <summary>
    ///     Ctor
    /// </summary>
    public SchedulerAdminController(IOptions<SchedulerOptions> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     任务定义列表
    /// </summary>
    [HttpGet("jobs")]
    [Description("任务定义列表")]
    public async Task<CommonResult<List<ScheduledJob>>> JobsAsync(CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<List<ScheduledJob>>(authResult.Code, authResult.Msg);
        var list = await _storage.GetListAsync(cancellationToken);
        return CommonResult.Success(list);
    }

    /// <summary>
    ///     新增或修改任务定义
    ///     <para>新增时按 Cron 计算首次触发；修改时若 Cron 变化则重算下次触发，未变化保持原锚点。</para>
    /// </summary>
    [HttpPost("jobs/save")]
    [Description("保存任务定义")]
    public async Task<CommonResult<ScheduledJob>> SaveJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<ScheduledJob>(authResult.Code, authResult.Msg);

        if (dto == null || string.IsNullOrWhiteSpace(dto.JobName) || string.IsNullOrWhiteSpace(dto.CronExpression))
            return CommonResult.Fail<ScheduledJob>("400", "JobName 与 CronExpression 必填");

        DateTime? nextRunTime;
        try
        {
            var next = CronUtility.GetNextOccurrence(dto.CronExpression, DateTimeOffset.Now);
            nextRunTime = next == default ? null : next;
        }
        catch (Exception ex)
        {
            return CommonResult.Fail<ScheduledJob>("400", $"Cron 表达式非法：{ex.Message}");
        }

        if (nextRunTime == null)
            return CommonResult.Fail<ScheduledJob>("400", "Cron 表达式无法计算出下次触发时间");

        if (dto.Id is > 0)
        {
            var existing = await _storage.GetAsync(dto.Id.Value, cancellationToken);
            if (existing == null) return CommonResult.Fail<ScheduledJob>("404", "任务不存在");

            var cronChanged = !string.Equals(existing.CronExpression, dto.CronExpression, StringComparison.Ordinal);
            existing.JobName = dto.JobName;
            existing.HandlerType = dto.HandlerType;
            existing.CronExpression = dto.CronExpression;
            existing.Parameter = dto.Parameter;
            existing.Remark = dto.Remark;
            if (cronChanged) existing.NextRunTime = nextRunTime;

            await _storage.UpdateAsync(existing, cancellationToken);
            return CommonResult.Success(existing);
        }

        var job = new ScheduledJob
        {
            JobName = dto.JobName,
            HandlerType = dto.HandlerType,
            CronExpression = dto.CronExpression,
            Parameter = dto.Parameter,
            Remark = dto.Remark,
            Status = JobStatus.Normal,
            NextRunTime = nextRunTime,
            TryCount = 0
        };

        await _storage.AddAsync(job, cancellationToken);
        return CommonResult.Success(job);
    }

    /// <summary>
    ///     删除任务定义
    /// </summary>
    [HttpPost("jobs/remove")]
    [Description("删除任务定义")]
    public async Task<CommonResult> RemoveJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;
        if (dto?.Id is not > 0) return CommonResult.Fail("400", "Id 必填");

        var affected = await _storage.DeleteAsync(dto.Id.Value, cancellationToken);
        return affected > 0 ? CommonResult.Success() : CommonResult.Fail("404", "任务不存在");
    }

    /// <summary>
    ///     暂停任务（仅停止，不隔离）
    /// </summary>
    [HttpPost("jobs/pause")]
    [Description("暂停任务")]
    public async Task<CommonResult> PauseJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;
        if (dto?.Id is not > 0) return CommonResult.Fail("400", "Id 必填");

        var job = await _storage.GetAsync(dto.Id.Value, cancellationToken);
        if (job == null) return CommonResult.Fail("404", "任务不存在");
        if (job.Status == JobStatus.Dead) return CommonResult.Fail("409", "任务已死信隔离，请先放行");

        job.Status = JobStatus.Stop;
        await _storage.UpdateAsync(job, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     恢复任务
    /// </summary>
    [HttpPost("jobs/resume")]
    [Description("恢复任务")]
    public async Task<CommonResult> ResumeJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;
        if (dto?.Id is not > 0) return CommonResult.Fail("400", "Id 必填");

        var job = await _storage.GetAsync(dto.Id.Value, cancellationToken);
        if (job == null) return CommonResult.Fail("404", "任务不存在");
        if (job.Status == JobStatus.Dead) return CommonResult.Fail("409", "任务已死信隔离，请走放行接口");

        var next = CronUtility.GetNextOccurrence(job.CronExpression, DateTimeOffset.Now);
        if (next == default) return CommonResult.Fail("400", "Cron 表达式无法计算出下次触发时间");

        job.Status = JobStatus.Normal;
        job.NextRunTime = next;
        job.RetryAtTime = null;
        await _storage.UpdateAsync(job, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     死信放行：把 Dead 任务恢复为 Normal，按 Cron 计算新的触发时刻
    /// </summary>
    [HttpPost("jobs/release")]
    [Description("死信放行")]
    public async Task<CommonResult> ReleaseJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;
        if (dto?.Id is not > 0) return CommonResult.Fail("400", "Id 必填");

        var job = await _storage.GetAsync(dto.Id.Value, cancellationToken);
        if (job == null) return CommonResult.Fail("404", "任务不存在");

        var next = CronUtility.GetNextOccurrence(job.CronExpression, DateTimeOffset.Now);
        if (next == default) return CommonResult.Fail("400", "Cron 表达式无法计算出下次触发时间");

        var affected = await _storage.ReleaseAsync(job.Id, next, cancellationToken);
        return affected > 0 ? CommonResult.Success() : CommonResult.Fail("409", "任务不是死信隔离状态");
    }

    /// <summary>
    ///     立即触发一次（不改变周期锚点）
    /// </summary>
    [HttpPost("jobs/trigger")]
    [Description("立即触发一次")]
    public async Task<CommonResult<string>> TriggerJobAsync([FromBody] SchedulerJobDto dto, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<string>(authResult.Code, authResult.Msg);
        if (dto?.Id is not > 0) return CommonResult.Fail<string>("400", "Id 必填");

        var job = await _storage.GetAsync(dto.Id.Value, cancellationToken);
        if (job == null) return CommonResult.Fail<string>("404", "任务不存在");
        if (job.Status == JobStatus.Dead) return CommonResult.Fail<string>("409", "任务已死信隔离，请先放行");

        var execution = await _claimer.OpenManualAsync(job, cancellationToken);
        if (execution == null) return CommonResult.Fail<string>("500", "创建执行记录失败");

        var ack = await _dispatcher.DispatchAsync(execution, cancellationToken);
        if (ack != DispatchAck.Accepted)
        {
            var message = ack.ToMessage();
            await _ledger.TryFinalizeAsync(execution.TraceId, ExecutionState.Failed, message, _clock.Now, null, cancellationToken);
            return CommonResult.Fail<string>("503", $"{message}（TraceId={execution.TraceId}）");
        }

        return CommonResult.Success<string>($"已投递，TraceId={execution.TraceId}");
    }

    /// <summary>
    ///     执行记录查询
    /// </summary>
    [HttpGet("executions")]
    [Description("执行记录")]
    public async Task<CommonResult<IReadOnlyList<SchedulerExecution>>> ExecutionsAsync(long jobId, int limit = 50,
        bool includeLogs = false, CancellationToken cancellationToken = default)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<IReadOnlyList<SchedulerExecution>>(authResult.Code, authResult.Msg);
        if (jobId <= 0) return CommonResult.Fail<IReadOnlyList<SchedulerExecution>>("400", "jobId 必填");

        limit = Math.Clamp(limit, 1, MaxExecutionPageSize);
        var list = await _ledger.ListByJobAsync(jobId, limit, cancellationToken);

        //  日志列默认不返回：单条执行日志可达数万字符，列表接口只给状态与耗时，详情按 TraceId 单独取
        if (!includeLogs)
        {
            var brief = new List<SchedulerExecution>(list.Count);
            foreach (var execution in list)
            {
                brief.Add(new SchedulerExecution
                {
                    Id = execution.Id,
                    TraceId = execution.TraceId,
                    ExecutionKey = execution.ExecutionKey,
                    JobId = execution.JobId,
                    JobName = execution.JobName,
                    HandlerType = execution.HandlerType,
                    Period = execution.Period,
                    Attempt = execution.Attempt,
                    State = execution.State,
                    WorkerAddress = execution.WorkerAddress,
                    DispatchAt = execution.DispatchAt,
                    FinishAt = execution.FinishAt,
                    ElapsedMs = execution.ElapsedMs,
                    Error = execution.Error,
                    Message = execution.Message
                });
            }

            return CommonResult.Success<IReadOnlyList<SchedulerExecution>>(brief);
        }

        return CommonResult.Success(list);
    }

    /// <summary>
    ///     单次执行详情：含结果消息与执行过程日志（按 TraceId 精确查询，用于事后复盘）
    /// </summary>
    [HttpGet("executions/{traceId}")]
    [Description("执行详情")]
    public async Task<CommonResult<SchedulerExecution>> ExecutionAsync(string traceId, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<SchedulerExecution>(authResult.Code, authResult.Msg);
        if (string.IsNullOrWhiteSpace(traceId)) return CommonResult.Fail<SchedulerExecution>("400", "traceId 必填");

        var execution = await _ledger.GetByTraceAsync(traceId, cancellationToken);
        return execution == null
            ? CommonResult.Fail<SchedulerExecution>("404", "执行记录不存在")
            : CommonResult.Success(execution);
    }

    /// <summary>
    ///     某次执行的过程日志（分页游标：afterId 之后的记录）
    ///     <para>日志在明细表按分片追加，量大时可反复带 afterId 拉取，不必一次取全。</para>
    /// </summary>
    [HttpGet("executions/{traceId}/logs")]
    [Description("执行过程日志")]
    public async Task<CommonResult<IReadOnlyList<SchedulerExecutionLog>>> ExecutionLogsAsync(string traceId,
        long afterId = 0, int limit = 200, CancellationToken cancellationToken = default)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<IReadOnlyList<SchedulerExecutionLog>>(authResult.Code, authResult.Msg);
        if (string.IsNullOrWhiteSpace(traceId)) return CommonResult.Fail<IReadOnlyList<SchedulerExecutionLog>>("400", "traceId 必填");

        limit = Math.Clamp(limit, 1, 500);
        var list = await _logStore.ListAsync(traceId, afterId, limit, cancellationToken);
        return CommonResult.Success(list);
    }

    /// <summary>
    ///     人工终止一次执行：先把执行行终结为 Cancelled（阻断后续回调重试），再通知执行节点取消
    /// </summary>
    [HttpPost("executions/kill")]
    [Description("终止执行")]
    public async Task<CommonResult> KillExecutionAsync([FromBody] KillRequest request, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;
        if (request == null || string.IsNullOrWhiteSpace(request.TraceId)) return CommonResult.Fail("400", "TraceId 必填");

        var execution = await _ledger.GetByTraceAsync(request.TraceId, cancellationToken);
        if (execution == null) return CommonResult.Fail("404", "执行记录不存在");
        if (ExecutionStateHelper.IsTerminal(execution.State)) return CommonResult.Fail("409", $"执行已终结：{execution.State}");

        if (!await _ledger.TryFinalizeAsync(execution.TraceId, ExecutionState.Cancelled, "人工终止", _clock.Now, null, cancellationToken))
            return CommonResult.Fail("409", "执行已被其他流程终结");

        if (!string.IsNullOrWhiteSpace(execution.WorkerAddress))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.Value.DispatchHttpTimeoutSeconds));
                Dictionary<string, string> headers = null;
                if (!string.IsNullOrWhiteSpace(_options.Value.ClusterToken))
                    headers = new Dictionary<string, string> { ["X-Scheduler-Token"] = _options.Value.ClusterToken };

                await client.PostAsJsonAsync($"{execution.WorkerAddress.TrimEnd('/')}/api/scheduler/jobs/kill",
                    request, headers, cancellationToken);
            }
            catch (Exception ex)
            {
                //  通知失败不影响结果：执行行已是终态，回调到达也会被忽略
                Logger.LogWarning(ex, "终止通知执行节点 {Address} 失败，TraceId={TraceId}", execution.WorkerAddress, request.TraceId);
            }
        }

        return CommonResult.Success();
    }

    /// <summary>
    ///     存活执行节点列表
    /// </summary>
    [HttpGet("workers")]
    [Description("执行节点列表")]
    public async Task<CommonResult<IReadOnlyList<WorkerNodeInfo>>> WorkersAsync(CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<IReadOnlyList<WorkerNodeInfo>>(authResult.Code, authResult.Msg);
        var since = _clock.Now.AddSeconds(-Math.Max(1, _options.Value.WorkerHeartbeatTtlSeconds));
        var list = await _directory.ListAliveAsync(since, cancellationToken);
        return CommonResult.Success(list);
    }

    /// <summary>
    ///     节点任务清单覆盖：哪些节点承载了哪些任务、哪些任务当前没有任何节点承载。
    ///     <para>各执行节点承载的任务可能不同，调度端按 HandlerType 路由；这里用于快速定位"任务没有节点能跑"的部署问题。</para>
    /// </summary>
    [HttpGet("capabilities")]
    [Description("节点任务清单覆盖")]
    public async Task<CommonResult<WorkerCapabilityReport>> CapabilitiesAsync(CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<WorkerCapabilityReport>(authResult.Code, authResult.Msg);

        var workers = _workerRegistry.Snapshot();
        var coverage = new SortedDictionary<string, WorkerTaskCoverage>(StringComparer.Ordinal);

        foreach (var worker in workers)
        {
            foreach (var capability in worker.Capabilities ?? [])
            {
                if (string.IsNullOrWhiteSpace(capability)) continue;

                if (!coverage.TryGetValue(capability, out var item))
                {
                    item = new WorkerTaskCoverage { HandlerType = capability };
                    coverage[capability] = item;
                }

                item.Nodes.Add(new WorkerNodeRef { WorkerId = worker.WorkerId, Name = worker.Name, Address = worker.Address });
            }
        }

        var report = new WorkerCapabilityReport
        {
            AliveWorkers = workers.Count,
            Tasks = coverage.Values.ToList()
        };

        var jobs = await _storage.GetListAsync(cancellationToken);
        foreach (var job in jobs)
        {
            if (string.IsNullOrWhiteSpace(job.HandlerType))
            {
                report.UncoveredJobs.Add(new UncoveredJob
                {
                    JobId = job.Id,
                    JobName = job.JobName,
                    HandlerType = job.HandlerType,
                    Status = job.Status,
                    NextRunTime = job.NextRunTime,
                    Reason = "未配置 HandlerType，无法路由到任何节点"
                });
                continue;
            }

            if (coverage.ContainsKey(job.HandlerType)) continue;

            report.UncoveredJobs.Add(new UncoveredJob
            {
                JobId = job.Id,
                JobName = job.JobName,
                HandlerType = job.HandlerType,
                Status = job.Status,
                NextRunTime = job.NextRunTime,
                Reason = workers.Count == 0 ? "当前无存活执行节点" : "存活节点均未注册该任务实现"
            });
        }

        return CommonResult.Success(report);
    }

    /// <summary>
    ///     当前主节点
    /// </summary>
    [HttpGet("master")]
    [Description("当前主节点")]
    public async Task<CommonResult<string>> MasterAsync(CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return CommonResult.Fail<string>(authResult.Code, authResult.Msg);
        var owner = await _lease.GetCurrentOwnerAsync(cancellationToken);
        return CommonResult.Success<string>(owner);
    }

    /// <summary>
    ///     共享令牌校验
    /// </summary>
    private bool TryAuthorize(out CommonResult result)
    {
        result = null;
        if (SchedulerTokenValidator.Validate(HttpContext, _options.Value.ClusterToken)) return true;

        result = CommonResult.Fail("auth.fail", "调度令牌无效");
        return false;
    }
}
