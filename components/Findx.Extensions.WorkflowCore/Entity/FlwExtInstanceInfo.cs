using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     扩展流程实例表
/// </summary>
[Table("flw_instance_ext")]
[EntityExtension(DataSource = "Workflow")]
[Description("扩展流程实例表")]
public class FlwExtInstanceInfo: EntityBase<long>
{
    /// <summary>
    ///     程实例ID
    /// </summary>
    public long InstanceId { get; set; }
    
    /// <summary>
    ///     流程定义ID
    /// </summary>
    public long ProcessId { get; set; }
    
    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }
    
    /// <summary>
    ///     流程类型
    /// </summary>
    public ProcessType ProcessType { get; set; }
    
    /// <summary>
    ///     流程模型定义JSON内容
    /// </summary>
    public string ModelContent { get; set; }
}