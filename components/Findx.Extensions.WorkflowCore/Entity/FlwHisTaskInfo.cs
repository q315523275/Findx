using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     历史任务表（已完成/拒绝/撤销/终止的任务归档）
/// </summary>
[Table("flw_task_his")]
[EntityExtension(DataSource = "Workflow")]
[Description("历史任务表")]
public class FlwHisTaskInfo: FlwTaskInfo
{
    /// <summary>
    ///     调用外部流程定义ID（子流程场景）
    /// </summary>
    public long CallProcessId { get; set; }

    /// <summary>
    ///     调用外部流程实例ID（子流程场景）
    /// </summary>
    public long CallInstanceId { get; set; }

    /// <summary>
    ///     完成时间
    /// </summary>
    public DateTime? FinishTime { get; set; }

    /// <summary>
    ///     任务状态
    /// </summary>
    public FlwTaskState TaskState { get; set; }

    /// <summary>
    ///     处理耗时（秒）
    /// </summary>
    public decimal Duration { get; set; }

    /// <summary>
    ///     审批意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     处理动作
    /// </summary>
    public FlwTaskAction Action { get; set; }
}
