namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     发起流程参数
/// </summary>
public class FlwStartArgs
{
    /// <summary>
    ///     流程编码（与 ProcessId 二选一）
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程定义ID（与 ProcessKey 二选一，优先使用）
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
    ///     发起人
    /// </summary>
    public FlwCreator Creator { get; set; }

    /// <summary>
    ///     期望完成时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    ///     流程实例编号（为空时自动生成）
    /// </summary>
    public string InstanceNo { get; set; }

    /// <summary>
    ///     优先级
    /// </summary>
    public int Priority { get; set; }
}
