using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     流程模型（process 表中 modelContent 反序列化后的根对象）
/// </summary>
public class FlwModel
{
    /// <summary>
    ///     流程编码
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    ///     流程类型
    /// </summary>
    public ProcessType ProcessType { get; set; } = ProcessType.Business;

    /// <summary>
    ///     根节点（发起节点，type=0）
    /// </summary>
    public FlwNodeModel Root { get; set; }
}
