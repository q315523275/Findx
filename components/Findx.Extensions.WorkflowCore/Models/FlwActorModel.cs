namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     节点办理人模型（流程模型 JSON 中 props.assignee 的元素）
/// </summary>
public class FlwActorModel
{
    /// <summary>
    ///     办理人类型：user 用户 / role 角色 / dept 部门 / creator 发起人 / superior 发起人上级
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     办理人标识（用户ID、角色编码、部门ID；creator/superior 可空）
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     办理人名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     票签权重（会签按权重计票时使用，默认 1）
    /// </summary>
    public int Weight { get; set; } = 1;
}
