using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     历史流程实例表
/// </summary>
[Table("flw_instance_his")]
[EntityExtension(DataSource = "Workflow")]
[Description("历史流程实例表")]
public class FlwHisInstanceInfo: FullEntityBase
{
    /// <summary>
    ///     流程定义ID
    /// </summary>
    public long ProcessId { get; set; }
    
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
    
    /// <summary>
    ///     状态 0:审批中 1:审批通过 2:审批拒绝 3:撤销审批 4:超时结束 5:强制终止
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    ///     结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    ///     处理耗时
    /// </summary>
    public decimal Duration { get; set; }
}