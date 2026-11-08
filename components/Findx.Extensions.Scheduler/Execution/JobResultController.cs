using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     调度端接口：接收 Worker 回报的执行结果，驱动重试/死信决策
/// </summary>
[Area("findx")]
[Route("api/scheduler/jobs")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度任务结果回调")]
public class JobResultController : AreaApiControllerBase
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly JobResultHandler _resultHandler;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobResultController(IOptions<SchedulerOptions> options, JobResultHandler resultHandler)
    {
        _options = options;
        _resultHandler = resultHandler;
    }

    /// <summary>
    ///     接收 Worker 执行结果回报
    /// </summary>
    [HttpPost("result")]
    [Description("接收执行结果")]
    public async Task<CommonResult> ResultAsync([FromBody] JobResultReport report, CancellationToken cancellationToken)
    {
        var token = _options.Value.ClusterToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            var headerToken = HttpContext.Request.Headers["X-Scheduler-Token"].FirstOrDefault();
            if (!string.Equals(headerToken, token, StringComparison.Ordinal))
                return CommonResult.Fail("auth.fail", "调度令牌无效");
        }

        await _resultHandler.HandleAsync(report, cancellationToken);
        return CommonResult.Success();
    }
}
