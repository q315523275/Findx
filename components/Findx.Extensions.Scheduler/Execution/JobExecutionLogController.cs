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
///     执行日志接收接口：执行端按分片批量上报执行过程日志，调度端追加落库。
///     <para>与结果回调分离：日志量大、产生频繁，不能占用结果回调的体积与延迟预算。</para>
/// </summary>
[Area("findx")]
[Route("api/scheduler/jobs")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度执行日志")]
[MasterRoleOnly]
public class JobExecutionLogController : AreaApiControllerBase
{
    /// <summary>
    ///     单次请求最多接收的分片数（防御超大请求体）
    /// </summary>
    private const int MaxChunksPerRequest = 64;

    private readonly IOptions<SchedulerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     日志明细存储（仅调度端注册）
    /// </summary>
    private IExecutionLogStore LogStore => LazyServices.Require<IExecutionLogStore>(_serviceProvider);

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobExecutionLogController(IOptions<SchedulerOptions> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     接收执行日志分片（幂等性由执行端顺序与主键追加保证，重复上报最多产生重复行）
    /// </summary>
    /// <param name="chunks"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("logs")]
    [Description("接收执行日志")]
    public async Task<CommonResult> LogsAsync([FromBody] List<ExecutionLogChunk> chunks, CancellationToken cancellationToken)
    {
        if (!_options.Value.SchedulerEnabled)
            return CommonResult.Fail("503", "本节点未启用调度端（Findx:Scheduler:SchedulerEnabled=false），无法接收执行日志");

        if (!TryAuthorize(out var authResult)) return authResult;
        if (chunks == null || chunks.Count == 0) return CommonResult.Fail("400", "日志分片为空");
        if (chunks.Count > MaxChunksPerRequest) return CommonResult.Fail("400", $"单次最多 {MaxChunksPerRequest} 片");

        var written = await LogStore.AppendAsync(chunks, cancellationToken);
        return CommonResult.Success(written);
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
