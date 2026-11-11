using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     节点属性（流程模型 JSON 中节点的 props）
/// </summary>
public class FlwNodeProps
{
    /// <summary>
    ///     办理人集合
    /// </summary>
    public List<FlwActorModel> Assignee { get; set; }

    /// <summary>
    ///     参与类型（单人/会签/或签/顺签）
    /// </summary>
    public FlwPerformType PerformType { get; set; } = FlwPerformType.Single;

    /// <summary>
    ///     期望完成天数（0 表示不限）
    /// </summary>
    public int ExpireDay { get; set; }

    /// <summary>
    ///     到期前提前提醒天数（0 表示不提醒）
    /// </summary>
    public int RemindDay { get; set; }

    /// <summary>
    ///     提醒重复间隔（分钟，0 表示只提醒一次）
    /// </summary>
    public int RemindRepeat { get; set; }

    /// <summary>
    ///     条件表达式（条件分支、路由分支使用）
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    ///     路由分支条件成立时跳转的目标节点 key
    /// </summary>
    public string TargetNodeKey { get; set; }

    /// <summary>
    ///     无人办理时是否自动通过
    /// </summary>
    public bool AutoApprove { get; set; }

    /// <summary>
    ///     触发器类型（触发器节点使用，如 timer / http / internal）
    /// </summary>
    public string TriggerType { get; set; }

    /// <summary>
    ///     触发器参数（触发器节点使用）
    /// </summary>
    public string TriggerArgs { get; set; }

    /// <summary>
    ///     延时时长（触发器节点使用，格式 数字+单位，如 7:d）
    /// </summary>
    public string DelayTime { get; set; }

    /// <summary>
    ///     子流程编码（子流程节点使用）
    /// </summary>
    public string CallProcessKey { get; set; }

    /// <summary>
    ///     扩展属性（保留前端设计器自定义字段）
    /// </summary>
    public Dictionary<string, object> Extend { get; set; }
}
