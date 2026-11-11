using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Dtos.Instance;

/// <summary>
///     发起流程参数
/// </summary>
public class InstanceStartDto: IRequest
{
    /// <summary>
    ///     流程编码（与 ProcessId 二选一）
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程定义ID
    /// </summary>
    public long ProcessId { get; set; }

    /// <summary>
    ///     业务编码
    /// </summary>
    public string BusinessKey { get; set; }

    /// <summary>
    ///     流程变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; }

    /// <summary>
    ///     流程实例编号（为空自动生成）
    /// </summary>
    public string InstanceNo { get; set; }

    /// <summary>
    ///     优先级
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    ///     期望完成时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }
}

/// <summary>
///     撤销流程参数
/// </summary>
public class InstanceRevokeDto: IRequest
{
    /// <summary>
    ///     流程实例ID
    /// </summary>
    [Required]
    public long InstanceId { get; set; }

    /// <summary>
    ///     撤销原因
    /// </summary>
    public string Comment { get; set; }
}

/// <summary>
///     实例分页查询
/// </summary>
public class InstancePageQueryDto: IRequest
{
    /// <summary>
    ///     页码
    /// </summary>
    public int PageNo { get; set; } = 1;

    /// <summary>
    ///     每页条数
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    ///     流程编码
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     业务编码
    /// </summary>
    public string BusinessCode { get; set; }

    /// <summary>
    ///     实例状态
    /// </summary>
    public int? InstanceState { get; set; }
}
