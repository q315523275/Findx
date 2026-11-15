using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Dtos.Component;
using Findx.Extensions.WorkflowCore.Dtos.Instance;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Services;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.WorkflowCore.Controllers;

/// <summary>
///     工作流-审批组件
///     <para>面向业务单据：一个 panel 接口渲染审批面板，submit/action/revoke 三个写接口完成全部交互</para>
/// </summary>
[Area("flw")]
[Route("api/[area]/component")]
[Authorize]
[ApiExplorerSettings(GroupName = "Workflow"), Description("工作流-审批组件")]
public class ComponentController: FlwControllerBase
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ComponentController(IWorkContext workContext): base(workContext)
    {
    }

    /// <summary>
    ///     审批面板：按业务单据一次性返回状态、时间轴、可用动作、当前待办
    /// </summary>
    [HttpGet("panel")]
    [Description("审批面板")]
    public async Task<CommonResult> PanelAsync([FromQuery] string businessType, [FromQuery] long businessId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(businessType)) return CommonResult.Fail("400", "businessType 不能为空");

        var service = GetRequiredService<FlwPanelService>();
        var panel = await service.GetPanelAsync(businessType, businessId, CurrentActor, cancellationToken);
        return CommonResult.Success(panel);
    }

    /// <summary>
    ///     我的待办（带业务锚点）
    /// </summary>
    [HttpGet("todo")]
    [Description("我的待办")]
    public async Task<CommonResult> TodoAsync([FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetTodoListAsync(CurrentActor, top, cancellationToken));
    }

    /// <summary>
    ///     我的已办（带业务锚点）
    /// </summary>
    [HttpGet("done")]
    [Description("我的已办")]
    public async Task<CommonResult> DoneAsync([FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetDoneListAsync(CurrentActor, top, cancellationToken));
    }

    /// <summary>
    ///     待办与已办数量（角标用）
    /// </summary>
    [HttpGet("count")]
    [Description("待办角标")]
    public async Task<CommonResult> CountAsync(CancellationToken cancellationToken)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetCountAsync(CurrentActor, cancellationToken));
    }

    /// <summary>
    ///     发起审批：业务单号/流程编码可省略，按业务类型匹配已发布流程
    /// </summary>
    [HttpPost("submit")]
    [Description("发起审批")]
    public async Task<CommonResult> SubmitAsync([FromBody] ComponentStartDto req, CancellationToken cancellationToken)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.BusinessType)) return CommonResult.Fail("400", "businessType 不能为空");

        var service = GetRequiredService<FlwPanelService>();
        var engine = GetRequiredService<IFlowLongEngine>();

        var panel = await service.GetPanelAsync(req.BusinessType, req.BusinessId, CurrentActor, cancellationToken);
        if (panel.Started) return CommonResult.Fail("flow.started", "该单据已发起审批，请勿重复发起");

        var processKey = req.ProcessKey;
        if (string.IsNullOrWhiteSpace(processKey))
        {
            var process = await service.FindProcessAsync(req.BusinessType, cancellationToken);
            processKey = process?.Code;
        }
        if (string.IsNullOrWhiteSpace(processKey)) return CommonResult.Fail("flow.not.found", $"业务类型[{req.BusinessType}]未配置已发布的审批流程");

        try
        {
            var instance = await engine.StartAsync(new FlwStartArgs
            {
                ProcessKey = processKey,
                BusinessType = req.BusinessType,
                BusinessId = req.BusinessId,
                BusinessKey = req.BusinessCode ?? panel.Business?.BusinessCode,
                Variables = req.Variables,
                Creator = CurrentActor
            }, cancellationToken);
            return CommonResult.Success(instance);
        }
        catch (FlwEngineException ex)
        {
            return CommonResult.Fail("flow.start.fail", ex.Message);
        }
    }

    /// <summary>
    ///     执行审批动作：approve / reject / transfer / addSign / jump
    /// </summary>
    [HttpPost("action")]
    [Description("审批动作")]
    public async Task<CommonResult> ActionAsync([FromBody] ComponentActionDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.TaskId <= 0) return CommonResult.Fail("400", "taskId 不能为空");

        var engine = GetRequiredService<IFlowLongEngine>();
        var action = (req.Action ?? string.Empty).Trim().ToLowerInvariant();
        try
        {
            switch (action)
            {
                case "approve":
                    await engine.ApproveAsync(req.TaskId, CurrentActor, req.Comment, req.Variables, cancellationToken);
                    break;
                case "reject":
                    await engine.RejectAsync(req.TaskId, CurrentActor, req.Comment, req.TargetNodeKey, req.EndProcess, cancellationToken);
                    break;
                case "transfer":
                    if (string.IsNullOrWhiteSpace(req.TargetActorId)) return CommonResult.Fail("400", "转办目标人不能为空");
                    await engine.TransferAsync(req.TaskId, CurrentActor, new FlwCreator(req.TargetActorId, req.TargetActorName), cancellationToken);
                    break;
                case "addSign":
                    await engine.AddSignAsync(req.TaskId, CurrentActor, req.Actors, cancellationToken);
                    break;
                case "jump":
                    if (string.IsNullOrWhiteSpace(req.TargetNodeKey)) return CommonResult.Fail("400", "跳转目标节点不能为空");
                    await engine.JumpAsync(req.TaskId, CurrentActor, req.TargetNodeKey, req.Comment, cancellationToken);
                    break;
                default:
                    return CommonResult.Fail("400", $"不支持的动作：{req.Action}");
            }
        }
        catch (FlwEngineException ex)
        {
            return CommonResult.Fail("flow.action.fail", ex.Message);
        }

        return CommonResult.Success();
    }

    /// <summary>
    ///     撤销审批：仅发起人，审批中的单据可撤销
    /// </summary>
    [HttpPost("revoke")]
    [Description("撤销审批")]
    public async Task<CommonResult> RevokeAsync([FromBody] InstanceRevokeDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.InstanceId <= 0) return CommonResult.Fail("400", "instanceId 不能为空");

        var engine = GetRequiredService<IFlowLongEngine>();
        try
        {
            await engine.RevokeAsync(req.InstanceId, CurrentActor, req.Comment, cancellationToken);
        }
        catch (FlwEngineException ex)
        {
            return CommonResult.Fail("flow.revoke.fail", ex.Message);
        }

        return CommonResult.Success();
    }
}
