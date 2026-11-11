using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     流程实例表
/// </summary>
[Table("flw_instance")]
[EntityExtension(DataSource = "Workflow")]
[Description("流程实例表")]
public class FlwInstanceInfo: FullEntityBase
{
    /// <summary>
    ///     流程定义ID
    /// </summary>
    public long ProcessId { get; set; }

    /// <summary>
    ///     流程编码（冗余存储，避免每次按 ID 回查定义表）
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    ///     实例状态（FlowLong 原生只在历史实例表记录状态，这里冗余一份以便运行中查询）
    /// </summary>
    public FlwInstanceState InstanceState { get; set; }

    /// <summary>
    ///     父流程实例ID
    /// </summary>
    public long ParentInstanceId { get; set; }
    
    /// <summary>
    ///     优先级
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    ///     流程实例编号
    /// </summary>
    public string InstanceNo { get; set; }
    
    /// <summary>
    ///     业务编码
    /// </summary>
    public string BusinessCode { get; set; }
    
    /// <summary>
    ///     变量json
    /// </summary>
    public string Variable { get; set; }
    
    /// <summary>
    ///     当前所在节点名称
    /// </summary>
    public string CurrentNodeName { get; set; }
    
    /// <summary>
    ///     当前所在节点key
    /// </summary>
    public string CurrentNodeCode { get; set; }
    
    /// <summary>
    ///     期望完成时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }
}