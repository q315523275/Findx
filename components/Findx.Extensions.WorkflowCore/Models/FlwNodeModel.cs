using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     流程节点模型（FlowLong 风格 JSON 树的节点）
/// </summary>
public class FlwNodeModel
{
    /// <summary>
    ///     节点名称
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    ///     节点 key（流程内唯一）
    /// </summary>
    public string NodeKey { get; set; }

    /// <summary>
    ///     节点类型
    /// </summary>
    public FlwNodeType Type { get; set; }

    /// <summary>
    ///     节点属性
    /// </summary>
    public FlwNodeProps Props { get; set; }

    /// <summary>
    ///     后继节点（顺序流转；分支节点表示分支汇聚后的节点）
    /// </summary>
    public FlwNodeModel ChildNode { get; set; }

    /// <summary>
    ///     条件分支的子分支（type=3 时使用）
    /// </summary>
    public List<FlwNodeModel> ConditionNodes { get; set; }

    /// <summary>
    ///     并行/包容分支的子分支（type=4/5 时使用）
    /// </summary>
    public List<FlwNodeModel> ParallelNodes { get; set; }

    /// <summary>
    ///     所属分支节点的 nodeKey（解析时按树结构标记，非分支内节点为 null）
    /// </summary>
    public string BranchParentKey { get; set; }

    /// <summary>
    ///     所属分支的根节点 nodeKey（分支内任意节点都指向该分支的根，用于汇聚到达判定）
    /// </summary>
    public string BranchRootKey { get; set; }

    /// <summary>
    ///     是否为分支节点
    /// </summary>
    public bool IsBranch()
    {
        return Type == FlwNodeType.Condition || Type == FlwNodeType.Parallel || Type == FlwNodeType.Inclusive;
    }

    /// <summary>
    ///     取分支列表（按类型归一）
    /// </summary>
    public List<FlwNodeModel> GetBranchNodes()
    {
        if (Type == FlwNodeType.Condition) return ConditionNodes ?? new List<FlwNodeModel>();
        if (Type == FlwNodeType.Parallel || Type == FlwNodeType.Inclusive) return ParallelNodes ?? new List<FlwNodeModel>();
        return new List<FlwNodeModel>();
    }

    /// <summary>
    ///     深度遍历节点树（含分支子树与后继链）
    /// </summary>
    public IEnumerable<FlwNodeModel> Traverse()
    {
        yield return this;
        foreach (var branch in GetBranchNodes())
        {
            foreach (var node in branch.Traverse()) yield return node;
        }
        if (ChildNode != null)
        {
            foreach (var node in ChildNode.Traverse()) yield return node;
        }
    }
}
