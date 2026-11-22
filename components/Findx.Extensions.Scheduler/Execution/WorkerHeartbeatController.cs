using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行端心跳接口：Worker 周期上报自身能力，调度端落库；下线时通知摘除。
///     <para>机器间接口，需携带 X-Scheduler-Token。</para>
/// </summary>
[Area("findx")]
[Route("api/scheduler/workers")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度节点注册")]
[MasterRoleOnly]
public class WorkerHeartbeatController : AreaApiControllerBase
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     节点注册表（仅调度端注册；纯执行端不会解析到这里）
    /// </summary>
    private IWorkerDirectory Directory => LazyServices.Require<IWorkerDirectory>(_serviceProvider);

    /// <summary>
    ///     Ctor
    /// </summary>
    public WorkerHeartbeatController(IOptions<SchedulerOptions> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     心跳上报
    /// </summary>
    [HttpPost("beat")]
    [Description("节点心跳")]
    public async Task<CommonResult> BeatAsync([FromBody] WorkerNodeInfo info, CancellationToken cancellationToken)
    {
        if (!_options.Value.SchedulerEnabled)
            return CommonResult.Fail("503", "本节点未启用调度端（Findx:Scheduler:SchedulerEnabled=false），无法接收心跳");

        if (!TryAuthorize(out var authResult)) return authResult;

        if (info == null || string.IsNullOrWhiteSpace(info.WorkerId))
            return CommonResult.Fail("400", "请求参数缺失");

        await Directory.HeartbeatAsync(info, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     下线通知
    /// </summary>
    [HttpPost("offline")]
    [Description("节点下线")]
    public async Task<CommonResult> OfflineAsync([FromBody] WorkerNodeInfo info, CancellationToken cancellationToken)
    {
        if (!_options.Value.SchedulerEnabled)
            return CommonResult.Fail("503", "本节点未启用调度端（Findx:Scheduler:SchedulerEnabled=false），无法处理下线通知");

        if (!TryAuthorize(out var authResult)) return authResult;

        if (info == null || string.IsNullOrWhiteSpace(info.WorkerId))
            return CommonResult.Fail("400", "请求参数缺失");

        await Directory.RemoveAsync(info.WorkerId, cancellationToken);
        Logger.LogInformation("Worker 节点 {Node} 已下线并摘除注册", info.Name ?? info.WorkerId);
        return CommonResult.Success();
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
