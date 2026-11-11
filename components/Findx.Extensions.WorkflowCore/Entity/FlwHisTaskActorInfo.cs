using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     历史任务参与者表（结构同活动表，用于流程结束后保留办理痕迹）
/// </summary>
[Table("flw_task_actor_his")]
[EntityExtension(DataSource = "Workflow")]
[Description("历史任务参与者表")]
public class FlwHisTaskActorInfo: FlwTaskActorInfo
{
}
