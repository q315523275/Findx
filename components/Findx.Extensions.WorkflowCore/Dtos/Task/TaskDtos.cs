using System.ComponentModel.DataAnnotations;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Dtos.Task;

/// <summary>
///     任务同意参数
/// </summary>
public class TaskApproveDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     审批意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     提交的流程变量（合并到实例变量，供后续条件分支使用）
    /// </summary>
    public Dictionary<string, object> Variables { get; set; }
}

/// <summary>
///     任务拒绝参数
/// </summary>
public class TaskRejectDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     审批意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     回退目标节点（为空则回退上一审批节点；无上一节点时结束流程）
    /// </summary>
    public string TargetNodeKey { get; set; }

    /// <summary>
    ///     是否直接结束流程（审批拒绝）
    /// </summary>
    public bool EndProcess { get; set; }
}

/// <summary>
///     任务转办参数
/// </summary>
public class TaskTransferDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     目标用户ID
    /// </summary>
    [Required]
    public string TargetActorId { get; set; }

    /// <summary>
    ///     目标用户名称
    /// </summary>
    public string TargetActorName { get; set; }
}

/// <summary>
///     任务加签参数
/// </summary>
public class TaskAddSignDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     加签人员
    /// </summary>
    [Required]
    public List<FlwActorModel> Actors { get; set; }
}

/// <summary>
///     任务跳转参数
/// </summary>
public class TaskJumpDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     目标节点 key
    /// </summary>
    [Required]
    public string TargetNodeKey { get; set; }

    /// <summary>
    ///     跳转说明
    /// </summary>
    public string Comment { get; set; }
}

/// <summary>
///     触发器任务完成参数
/// </summary>
public class TaskTriggerCompleteDto: IRequest
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Required]
    public long TaskId { get; set; }

    /// <summary>
    ///     回调说明
    /// </summary>
    public string Comment { get; set; }
}
