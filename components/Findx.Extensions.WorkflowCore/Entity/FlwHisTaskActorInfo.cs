using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using FsIndex = FreeSql.DataAnnotations.IndexAttribute;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     历史任务参与者表（结构同活动表，用于流程结束后保留办理痕迹）
/// </summary>
[Table("flw_task_actor_his")]
[FsIndex("idx_flw_his_actor_task", "TaskId")]
[FsIndex("idx_flw_his_actor_actor", "ActorId,InstanceId")]
[Description("历史任务参与者表")]
public class FlwHisTaskActorInfo: FlwTaskActorInfo;
