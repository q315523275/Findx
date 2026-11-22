using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     调度端接口：接收 Worker 回报的执行结果，驱动重试/死信决策
/// </summary>
[Area("findx")]
[Route("api/scheduler/jobs")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度任务结果回调")]
[MasterRoleOnly]
public class JobResultController : AreaApiControllerBase
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     结果处理器（仅调度端注册）
    /// </summary>
    private JobResultHandler ResultHandler => LazyServices.Require<JobResultHandler>(_serviceProvider);

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="serviceProvider">用于惰性解析调度端服务（纯执行端不会解析）</param>
    public JobResultController(IOptions<SchedulerOptions> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     接收 Worker 执行结果回报
    /// </summary>
    /// <param name="report"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("result")]
    [Description("接收执行结果")]
    public async Task<CommonResult> ResultAsync([FromBody] JobResultReport report, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return authResult;

        await ResultHandler.HandleAsync(report, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     批量接收 Worker 执行结果回报：逐条按 TraceId 幂等终结，单条异常不影响批内其余结果。
    ///     <para>有失败条时返回 500，让执行端整批重试（重复投递只会命中已终结的行并被忽略）；
    ///     仍然失败则交由执行账本超时兜底。</para>
    /// </summary>
    /// <param name="reports"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("results")]
    [Description("批量接收执行结果")]
    public async Task<IActionResult> ResultsAsync([FromBody] List<JobResultReport> reports, CancellationToken cancellationToken)
    {
        if (!TryAuthorize(out var authResult)) return new ObjectResult(authResult) { StatusCode = 401 };

        if (reports == null || reports.Count == 0)
            return new ObjectResult(CommonResult.Fail("400", "请求参数缺失")) { StatusCode = 400 };

        var failed = 0;
        foreach (var report in reports)
        {
            if (report == null || string.IsNullOrWhiteSpace(report.TraceId))
            {
                failed++;
                continue;
            }

            try
            {
                await ResultHandler.HandleAsync(report, cancellationToken);
            }
            catch (Exception ex)
            {
                //  单条失败不能拖垮整批：批内其余结果照常终结，失败条靠重试或超时兜底
                failed++;
                Logger.LogError(ex, "批量结果处理失败，TraceId={TraceId}", report.TraceId);
            }
        }

        if (failed > 0)
            return new ObjectResult(CommonResult.Fail("500", $"批量结果处理失败 {failed}/{reports.Count} 条")) { StatusCode = 500 };

        return new ObjectResult(CommonResult.Success()) { StatusCode = 200 };
    }

    /// <summary>
    ///     共享令牌校验（配置 ClusterToken 后启用）
    /// </summary>
    private bool TryAuthorize(out CommonResult result)
    {
        result = null;
        var token = _options.Value.ClusterToken;
        if (string.IsNullOrWhiteSpace(token)) return true;

        var headerToken = HttpContext.Request.Headers["X-Scheduler-Token"].FirstOrDefault();
        if (string.Equals(headerToken, token, StringComparison.Ordinal)) return true;

        result = CommonResult.Fail("auth.fail", "调度令牌无效");
        return false;
    }
}
