using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Extensions.WorkflowCore.Dtos.Instance;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.WorkflowCore.Controllers;

/// <summary>
///     工作流-流程实例
/// </summary>
[Area("flw")]
[Route("api/[area]/instance")]
[Authorize]
[ApiExplorerSettings(GroupName = "Workflow"), Description("工作流-流程实例")]
public class InstanceController: FlwControllerBase
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public InstanceController(IWorkContext workContext): base(workContext)
    {
    }

    /// <summary>
    ///     发起流程
    /// </summary>
    [HttpPost("start")]
    public async Task<CommonResult> StartAsync([FromBody] InstanceStartDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        var instance = await engine.StartAsync(new FlwStartArgs
        {
            ProcessKey = req.ProcessKey,
            ProcessId = req.ProcessId,
            BusinessKey = req.BusinessKey,
            Variables = req.Variables,
            InstanceNo = req.InstanceNo,
            Priority = req.Priority,
            ExpireTime = req.ExpireTime,
            Creator = CurrentActor
        }, cancellationToken);

        return CommonResult.Success(instance);
    }

    /// <summary>
    ///     撤销流程（仅发起人）
    /// </summary>
    [HttpPost("revoke")]
    public async Task<CommonResult> RevokeAsync([FromBody] InstanceRevokeDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.RevokeAsync(req.InstanceId, CurrentActor, req.Comment, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     实例详情（活动任务 + 历史任务）
    /// </summary>
    [HttpGet("detail")]
    public async Task<CommonResult> DetailAsync([FromQuery] long instanceId, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        var detail = await engine.GetInstanceDetailAsync(instanceId, cancellationToken);
        return CommonResult.Success(detail);
    }

    /// <summary>
    ///     实例分页
    /// </summary>
    [HttpGet("page")]
    public async Task<CommonResult> PageAsync([FromQuery] InstancePageQueryDto req, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwInstanceInfo, long>();
        var whereExpression = PredicateBuilder.New<FlwInstanceInfo>()
            .AndIf(req.ProcessKey.IsNotNullOrWhiteSpace(), x => x.ProcessKey == req.ProcessKey)
            .AndIf(req.BusinessCode.IsNotNullOrWhiteSpace(), x => x.BusinessCode == req.BusinessCode)
            .AndIf(req.InstanceState.HasValue, x => x.InstanceState == (FlwInstanceState)req.InstanceState.Value)
            .Build();

        var result = await repo.PagedAsync<FlwInstanceInfo>(req.PageNo, req.PageSize, whereExpression, cancellationToken: cancellationToken);
        return CommonResult.Success(result);
    }
}
