using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Dtos.Model;

/// <summary>
///     模型简化信息参数Dto
/// </summary>
public class ModelSimplifyDto
{
    /// <summary>
    ///     记录Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    ///     流程编码
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    ///     流程名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    ///     流程图标地址
    /// </summary>
    public string Icon { get; set; }
    
    /// <summary>
    ///     流程类型
    /// </summary>
    public ProcessType Type { get; set; }
    
    /// <summary>
    ///     流程版本
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    ///     实例地址
    /// </summary>
    public string InstanceUrl { get; set; }
    
    /// <summary>
    ///     使用范围 0:全员 1:指定人员(业务关联) 2:均不可提交
    /// </summary>
    public int UseScope { get; set; }
    
    /// <summary>
    ///     流程状态 0:不可用 1:可用 2:历史版本
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    ///     备注说明
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     分组名称
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     绑定的业务类型（如 pms.order）
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     是否已配置动态表单
    ///     <para>列表只关心有没有配，不传整段 FormSchema，避免列表接口返回体积过大</para>
    /// </summary>
    public bool HasForm { get; set; }

    /// <summary>
    ///     创建时间（版本历史里用来标时间）
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     最后更新时间
    /// </summary>
    public DateTime? LastUpdatedTime { get; set; }

    /// <summary>
    ///     最后更新人
    /// </summary>
    public string LastUpdater { get; set; }
}