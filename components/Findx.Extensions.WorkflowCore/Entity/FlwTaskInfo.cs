using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     任务表（活动任务）
/// </summary>
[Table("flw_task")]
[EntityExtension(DataSource = "Workflow")]
[Description("任务表")]
public class FlwTaskInfo: EntityBase, ITenant
{
    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     父任务ID（会签/并行场景下指向派生前的任务）
    /// </summary>
    public long ParentTaskId { get; set; }

    /// <summary>
    ///     任务名称（节点名称）
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    ///     任务节点 key（对应流程模型节点 nodeKey）
    /// </summary>
    public string TaskKey { get; set; }

    /// <summary>
    ///     任务类型
    /// </summary>
    public FlwTaskType TaskType { get; set; }

    /// <summary>
    ///     参与类型（单人/会签/或签/顺签）
    /// </summary>
    public FlwPerformType PerformType { get; set; }

    /// <summary>
    ///     任务处理地址（外置表单）
    /// </summary>
    public string ActionUrl { get; set; }

    /// <summary>
    ///     任务变量 json
    /// </summary>
    public string Variable { get; set; }

    /// <summary>
    ///     委托人ID
    /// </summary>
    public string AssignorId { get; set; }

    /// <summary>
    ///     委托人名称
    /// </summary>
    public string Assignor { get; set; }

    /// <summary>
    ///     期望完成时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    ///     提醒时间
    /// </summary>
    public DateTime? RemindTime { get; set; }

    /// <summary>
    ///     提醒次数
    /// </summary>
    public int RemindRepeat { get; set; }

    /// <summary>
    ///     已阅 0 否 1 是
    /// </summary>
    public int Viewed { get; set; }

    /// <summary>
    ///     租户
    /// </summary>
    public string TenantId { get; set; }
}
