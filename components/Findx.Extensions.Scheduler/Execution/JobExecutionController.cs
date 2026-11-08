using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行端接口：接收 Master 分发的任务，入队后立即返回受理结果；执行与结果回调由执行线程完成
/// </summary>
[Area("findx")]
[Route("api/scheduler/jobs")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度任务执行")]
public class JobExecutionController : AreaApiControllerBase
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly JobExecutionQueue _executionQueue;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobExecutionController(IOptions<SchedulerOptions> options, JobExecutionQueue executionQueue)
    {
        _options = options;
        _executionQueue = executionQueue;
    }

    /// <summary>
    ///     接收 Master 分发的任务：校验后入队即返回（受理语义），不在请求线程里执行 Job
    /// </summary>
    [HttpPost("execute")]
    [Description("受理任务执行")]
    public CommonResult ExecuteAsync([FromBody] DispatchRequest request, CancellationToken cancellationToken)
    {
        // 机器间接口共享令牌校验（配置 ClusterToken 后启用）
        if (!TryAuthorize(out var authResult)) return authResult;

        if (request == null || string.IsNullOrWhiteSpace(request.HandlerType))
            return CommonResult.Fail("400", "请求参数缺失");

        // 受理即返回：执行放到内存队列的消费线程，结果回调也由消费线程发出。
        // 这样 Master 侧分发等待时长 ≈ 一次轻量 HTTP，而不是整个 Job 的执行时长
        if (!_executionQueue.TryEnqueue(request))
        {
            Logger.LogWarning("任务 [{JobName}] 执行队列已满，拒绝受理，交由 Master 侧重试", request.JobName);
            return CommonResult.Fail("503", "执行队列已满，请稍后重试");
        }

        Logger.LogInformation("任务 [{JobName}] TraceId={TraceId} 已受理，等待执行", request.JobName, request.TraceId);
        return CommonResult.Success();
    }

    /// <summary>
    ///     共享令牌校验
    /// </summary>
    private bool TryAuthorize(out CommonResult result)
    {
        result = null;
        var token = _options.Value.ClusterToken;
        if (string.IsNullOrWhiteSpace(token)) return true;

        var headerToken = HttpContext.Request.Headers["X-Scheduler-Token"].FirstOrDefault();
        if (!string.Equals(headerToken, token, StringComparison.Ordinal))
        {
            result = CommonResult.Fail("auth.fail", "调度令牌无效");
            return false;
        }

        return true;
    }
}
