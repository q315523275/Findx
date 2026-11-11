using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Dtos.Task;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.WorkflowCore.Controllers;

/// <summary>
///     工作流-任务中心
/// </summary>
[Area("flw")]
[Route("api/[area]/task")]
[Authorize]
[ApiExplorerSettings(GroupName = "Workflow"), Description("工作流-任务中心")]
public class TaskController: FlwControllerBase
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public TaskController(IWorkContext workContext): base(workContext)
    {
    }

    /// <summary>
    ///     我的待办
    /// </summary>
    [HttpGet("todo")]
    public async Task<CommonResult> TodoAsync([FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        var tasks = await engine.GetTodoTasksAsync(CurrentActor, top, cancellationToken);
        return CommonResult.Success(tasks);
    }

    /// <summary>
    ///     我的已办
    /// </summary>
    [HttpGet("done")]
    public async Task<CommonResult> DoneAsync([FromQuery] int top = 100, CancellationToken cancellationToken = default)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        var tasks = await engine.GetDoneTasksAsync(CurrentActor, top, cancellationToken);
        return CommonResult.Success(tasks);
    }

    /// <summary>
    ///     同意
    /// </summary>
    [HttpPost("approve")]
    public async Task<CommonResult> ApproveAsync([FromBody] TaskApproveDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.ApproveAsync(req.TaskId, CurrentActor, req.Comment, req.Variables, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     拒绝（默认回退上一审批节点）
    /// </summary>
    [HttpPost("reject")]
    public async Task<CommonResult> RejectAsync([FromBody] TaskRejectDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.RejectAsync(req.TaskId, CurrentActor, req.Comment, req.TargetNodeKey, req.EndProcess, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     转办
    /// </summary>
    [HttpPost("transfer")]
    public async Task<CommonResult> TransferAsync([FromBody] TaskTransferDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.TransferAsync(req.TaskId, CurrentActor, new FlwCreator(req.TargetActorId, req.TargetActorName), cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     加签
    /// </summary>
    [HttpPost("addSign")]
    public async Task<CommonResult> AddSignAsync([FromBody] TaskAddSignDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.AddSignAsync(req.TaskId, CurrentActor, req.Actors, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     跳转
    /// </summary>
    [HttpPost("jump")]
    public async Task<CommonResult> JumpAsync([FromBody] TaskJumpDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.JumpAsync(req.TaskId, CurrentActor, req.TargetNodeKey, req.Comment, cancellationToken);
        return CommonResult.Success();
    }

    /// <summary>
    ///     触发器任务完成回调（定时器/外部系统调用后继续流转）
    /// </summary>
    [HttpPost("completeTrigger")]
    public async Task<CommonResult> CompleteTriggerAsync([FromBody] TaskTriggerCompleteDto req, CancellationToken cancellationToken)
    {
        var engine = GetRequiredService<IFlowLongEngine>();
        await engine.CompleteTriggerAsync(req.TaskId, CurrentActor, req.Comment, cancellationToken);
        return CommonResult.Success();
    }
}
