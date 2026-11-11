using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     任务参与者表（活动任务的办理人/候选人）
/// </summary>
[Table("flw_task_actor")]
[EntityExtension(DataSource = "Workflow")]
[Description("任务参与者表")]
public class FlwTaskActorInfo: EntityBase, ITenant
{
    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     任务ID
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    ///     参与者ID
    /// </summary>
    public string ActorId { get; set; }

    /// <summary>
    ///     参与者名称
    /// </summary>
    public string ActorName { get; set; }

    /// <summary>
    ///     参与者类型（用户/角色/部门）
    /// </summary>
    public FlwActorType ActorType { get; set; }

    /// <summary>
    ///     权重（票签按比例计权；转办/委托任务为 1 表示代理人）
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    ///     代理人ID
    /// </summary>
    public string AgentId { get; set; }

    /// <summary>
    ///     代理类型 0 代理 1 被代理 2 认领角色 3 认领部门
    /// </summary>
    public int AgentType { get; set; }

    /// <summary>
    ///     扩展 json
    /// </summary>
    public string Ext { get; set; }

    /// <summary>
    ///     租户
    /// </summary>
    public string TenantId { get; set; }
}
