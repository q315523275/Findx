using System.Text.Json;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     流程模型解析器：把 process.modelContent 的 JSON 树解析为 <see cref="FlwModel"/> 并做结构校验
/// </summary>
public static class FlwModelParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    ///     解析流程模型 JSON
    /// </summary>
    /// <param name="modelContent">流程模型 JSON 内容</param>
    /// <param name="processKey">流程编码（JSON 中未带时回填）</param>
    /// <param name="processName">流程名称（JSON 中未带时回填）</param>
    /// <param name="processType">流程类型（JSON 中未带时回填）</param>
    public static FlwModel Parse(string modelContent, string processKey = null, string processName = null, ProcessType processType = ProcessType.Business)
    {
        if (string.IsNullOrWhiteSpace(modelContent))
            throw new FlwModelException("流程模型内容为空");

        FlwModel model;
        try
        {
            model = JsonSerializer.Deserialize<FlwModel>(modelContent, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new FlwModelException($"流程模型 JSON 解析失败：{ex.Message}");
        }

        if (model == null) throw new FlwModelException("流程模型 JSON 解析结果为空");

        model.ProcessKey ??= processKey;
        model.ProcessName ??= processName;
        if (model.ProcessType == default && processType != default) model.ProcessType = processType;

        Validate(model);
        return model;
    }

    /// <summary>
    ///     结构校验
    /// </summary>
    public static void Validate(FlwModel model)
    {
        if (model.Root == null)
            throw new FlwModelException("流程模型缺少根节点（root）");

        if (model.Root.Type != FlwNodeType.Root)
            throw new FlwModelException($"流程根节点类型必须为 {FlwNodeType.Root}（发起节点），当前为 {model.Root.Type}");

        var keys = new HashSet<string>(StringComparer.Ordinal);
        var nodes = model.Root.Traverse().ToList();

        foreach (var node in nodes)
        {
            if (string.IsNullOrWhiteSpace(node.NodeKey))
                throw new FlwModelException($"节点[{node.NodeName}]缺少 nodeKey");

            if (!keys.Add(node.NodeKey))
                throw new FlwModelException($"节点 key 重复：{node.NodeKey}");

            if (node.Type == FlwNodeType.Approval)
            {
                var hasAssignee = node.Props?.Assignee is { Count: > 0 };
                if (!hasAssignee && node.Props?.AutoApprove != true)
                    throw new FlwModelException($"审批节点[{node.NodeName}]未配置办理人（props.assignee），且未开启 autoApprove");
            }

            if (node.Type == FlwNodeType.CallProcess && string.IsNullOrWhiteSpace(node.Props?.CallProcessKey))
                throw new FlwModelException($"子流程节点[{node.NodeName}]缺少 callProcessKey");
        }

        //  先标记分支归属，再区分「分支容器」与「分支项」：分支项自身也可以是条件/并行类型，但它不带子分支列表
        MarkBranches(model.Root);

        foreach (var node in nodes.Where(x => x.IsBranch() && (x.BranchParentKey == null || x.GetBranchNodes().Count > 0)))
        {
            var branchNodes = node.GetBranchNodes();
            if (branchNodes.Count == 0)
                throw new FlwModelException($"分支节点[{node.NodeName}]缺少子分支");

            if (node.Type == FlwNodeType.Condition)
            {
                //  条件分支必须以无条件的默认分支收尾，否则全部条件不成立时流程无路可走
                if (!branchNodes.Any(x => string.IsNullOrWhiteSpace(x.Props?.Expression)))
                    throw new FlwModelException($"条件分支节点[{node.NodeName}]缺少默认分支（无 condition 的子分支）");
            }

            if (node.Type == FlwNodeType.Route)
            {
                foreach (var branch in branchNodes.Where(x => !string.IsNullOrWhiteSpace(x.Props?.Expression)))
                {
                    if (string.IsNullOrWhiteSpace(branch.Props?.TargetNodeKey))
                        throw new FlwModelException($"路由分支[{branch.NodeName}]缺少 targetNodeKey");
                }
            }
        }

        //  路由分支的 targetNodeKey 必须指向模型中真实存在的节点
        foreach (var node in nodes.Where(x => x.Type == FlwNodeType.Route))
        {
            foreach (var branch in node.GetBranchNodes().Where(x => !string.IsNullOrWhiteSpace(x.Props?.TargetNodeKey)))
            {
                if (!keys.Contains(branch.Props.TargetNodeKey))
                    throw new FlwModelException($"路由分支[{branch.NodeName}]的目标节点不存在：{branch.Props.TargetNodeKey}");
            }
        }
    }

    /// <summary>
    ///     标记分支归属：分支内任意节点记录所属分支节点与分支根，供并行/包容分支汇聚到达判定
    /// </summary>
    private static void MarkBranches(FlwNodeModel node)
    {
        foreach (var branch in node.GetBranchNodes())
        {
            MarkBranchTree(branch, node.NodeKey, branch.NodeKey);
        }
        if (node.ChildNode != null) MarkBranches(node.ChildNode);
    }

    private static void MarkBranchTree(FlwNodeModel node, string parentKey, string rootKey)
    {
        node.BranchParentKey = parentKey;
        node.BranchRootKey = rootKey;

        if (node.ChildNode != null) MarkBranchTree(node.ChildNode, parentKey, rootKey);

        //  嵌套分支：子分支归属该分支节点，根为子分支自身
        foreach (var sub in node.GetBranchNodes()) MarkBranchTree(sub, node.NodeKey, sub.NodeKey);
    }
}

/// <summary>
///     流程模型错误
/// </summary>
public class FlwModelException: Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwModelException(string message): base(message)
    {
    }
}
