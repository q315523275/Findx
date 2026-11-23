using System.ComponentModel;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Dtos.Component;
using Findx.Extensions.WorkflowCore.Dtos.Instance;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.ServiceDefaults;
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
    /// <param name="businessType"></param>
    /// <param name="businessId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
    ///     按实例获取审批面板（待办/已办页用，标准审批无业务锚点也能看）
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("instance-panel")]
    [Description("审批面板-按实例")]
    public async Task<CommonResult> InstancePanelAsync([FromQuery] long instanceId, CancellationToken cancellationToken)
    {
        if (instanceId <= 0) return CommonResult.Fail("400", "instanceId 不能为空");

        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetPanelByInstanceAsync(instanceId, CurrentActor, cancellationToken));
    }

    /// <summary>
    ///     催办：给当前在途节点的其他审批人下发提醒
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("urge")]
    [Description("催办")]
    public async Task<CommonResult> UrgeAsync([FromBody] ComponentUrgeDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.InstanceId <= 0) return CommonResult.Fail("400", "instanceId 不能为空");

        var service = GetRequiredService<FlwPanelService>();
        var count = await service.UrgeAsync(req.InstanceId, CurrentActor, req.Comment, cancellationToken);
        return CommonResult.Success(new { count });
    }

    /// <summary>
    ///     评论：不产生审批任务，只追加到实例变量
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("comment")]
    [Description("评论")]
    public async Task<CommonResult> CommentAsync([FromBody] ComponentCommentDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.InstanceId <= 0) return CommonResult.Fail("400", "instanceId 不能为空");
        if (string.IsNullOrWhiteSpace(req.Content)) return CommonResult.Fail("400", "评论内容不能为空");

        var service = GetRequiredService<FlwPanelService>();
        await service.AddCommentAsync(req.InstanceId, CurrentActor, req.Content, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     我的待办（带业务锚点）
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="top"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("todo")]
    [Description("我的待办")]
    public async Task<CommonResult> TodoAsync([FromQuery] int skip = 0, [FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetTodoListAsync(CurrentActor, skip, top, cancellationToken));
    }

    /// <summary>
    ///     我的已办（带业务锚点）
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="top"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("done")]
    [Description("我的已办")]
    public async Task<CommonResult> DoneAsync([FromQuery] int skip = 0, [FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetDoneListAsync(CurrentActor, skip, top, cancellationToken));
    }

    /// <summary>
    ///     发起审批页数据：统计 + 最近使用 + 分组流程
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("start-page")]
    [Description("发起审批页")]
    public async Task<CommonResult> StartPageAsync(CancellationToken cancellationToken)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetStartPageAsync(CurrentActor, cancellationToken));
    }

    /// <summary>
    ///     我发起的实例
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="top"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("applied")]
    [Description("我发起的")]
    public async Task<CommonResult> AppliedAsync([FromQuery] int skip = 0, [FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetAppliedListAsync(CurrentActor, skip, top, cancellationToken));
    }

    /// <summary>
    ///     我收到的实例（我参与但非我发起）
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="top"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("received")]
    [Description("我收到的")]
    public async Task<CommonResult> ReceivedAsync([FromQuery] int skip = 0, [FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetReceivedListAsync(CurrentActor, skip, top, cancellationToken));
    }

    /// <summary>
    ///     可发起的业务单据候选
    /// </summary>
    /// <param name="businessType"></param>
    /// <param name="keyword"></param>
    /// <param name="top"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("start-candidates")]
    [Description("可发起业务单据")]
    public async Task<CommonResult> StartCandidatesAsync([FromQuery] string businessType, [FromQuery] string keyword, [FromQuery] int top = 20, CancellationToken cancellationToken = default)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetStartCandidatesAsync(businessType, keyword, top, cancellationToken));
    }

    /// <summary>
    ///     待办与已办数量（角标用）
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("count")]
    [Description("待办角标")]
    public async Task<CommonResult> CountAsync(CancellationToken cancellationToken)
    {
        var service = GetRequiredService<FlwPanelService>();
        return CommonResult.Success(await service.GetCountAsync(CurrentActor, cancellationToken));
    }

    /// <summary>
    ///     发起审批：业务审批按"业务类型 + 业务主键"匹配流程；标准审批按 processKey 发起并携带动态表单数据
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("submit")]
    [Description("发起审批")]
    public async Task<CommonResult> SubmitAsync([FromBody] ComponentStartDto req, CancellationToken cancellationToken)
    {
        if (req == null) return CommonResult.Fail("400", "请求参数不能为空");

        var service = GetRequiredService<FlwPanelService>();
        var engine = GetRequiredService<IFlowEngine>();
        var isStandard = string.IsNullOrWhiteSpace(req.BusinessType);
        if (isStandard && string.IsNullOrWhiteSpace(req.ProcessKey))
            return CommonResult.Fail("400", "标准审批必须指定流程编码 processKey");

        //  业务审批要先确认单据未发起过；标准审批没有业务锚点，跳过这一步
        FlwPanelDto panel = null;
        if (!isStandard)
        {
            panel = await service.GetPanelAsync(req.BusinessType, req.BusinessId, CurrentActor, cancellationToken);
            //  CanStart 由业务适配器判定：审批中不可重复发起，已拒绝/已撤回允许重新发起
            if (!panel.CanStart) return CommonResult.Fail("flow.started", "该单据当前不可发起审批（审批中或状态不允许）");
        }

        var processKey = req.ProcessKey;
        if (string.IsNullOrWhiteSpace(processKey))
        {
            var process = await service.FindProcessAsync(req.BusinessType, cancellationToken);
            processKey = process?.Code;
        }
        if (string.IsNullOrWhiteSpace(processKey)) return CommonResult.Fail("flow.not.found", $"业务类型[{req.BusinessType}]未配置已发布的审批流程");

        //  标准审批的表单数据写进实例变量，与业务单据流程共用一套存储
        var variables = req.Variables ?? new Dictionary<string, object>();
        //  业务审批：先把单据字段并入变量（条件分支按业务字段求值时需要），表单数据随后覆盖
        await MergeBusinessVariablesAsync(variables, req.BusinessType, req.BusinessId, cancellationToken);
        if (req.FormData is { Count: > 0 })
        {
            variables["__form__"] = req.FormData;
            //  条件分支表达式写成 ${字段}，求值器只认顶层变量，因此把表单字段平铺一份
            //  （跳过 __ 前缀，避免与引擎保留的 __join__ / __branches__ 冲突）
            foreach (var item in req.FormData.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !x.Key.StartsWith("__")))
                variables[item.Key] = item.Value;
        }

        try
        {
            var instance = await engine.StartAsync(new FlwStartArgs
            {
                ProcessKey = processKey,
                BusinessType = req.BusinessType,
                BusinessId = req.BusinessId,
                BusinessKey = req.BusinessCode ?? panel?.Business?.BusinessCode,
                Variables = variables,
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
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("action")]
    [Description("审批动作")]
    public async Task<CommonResult> ActionAsync([FromBody] ComponentActionDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.TaskId <= 0) return CommonResult.Fail("400", "taskId 不能为空");

        var engine = GetRequiredService<IFlowEngine>();
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
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("revoke")]
    [Description("撤销审批")]
    public async Task<CommonResult> RevokeAsync([FromBody] InstanceRevokeDto req, CancellationToken cancellationToken)
    {
        if (req == null || req.InstanceId <= 0) return CommonResult.Fail("400", "instanceId 不能为空");

        var engine = GetRequiredService<IFlowEngine>();
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
