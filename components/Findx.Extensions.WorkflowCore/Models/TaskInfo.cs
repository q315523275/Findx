using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     任务表
/// </summary>
[Table("flw_process")]
[EntityExtension(DataSource = "Workflow")]
[Description("任务表")]
public class TaskInfo: FullEntityBase
{
    
}