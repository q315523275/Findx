namespace Findx.Data;

/// <summary>
///     树形数据构建者
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class TreeBuilder<TNode, TKey> where TNode : ITreeNode<TKey> where TKey : struct, IComparable
{
    /// <summary>
    ///     构造树节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="rootParentId"></param>
    /// <returns></returns>
    public IEnumerable<TNode> Build(List<TNode> nodes, TKey rootParentId)
    {
        nodes.ForEach(u => BuildChildNodes(nodes, u, []));

        foreach (var item in nodes.Where(item => rootParentId.Equals(item.GetParentId())))
        {
            yield return item;
        }
    }

    /// <summary>
    ///     构造子节点集合
    /// </summary>
    /// <param name="totalNodes"></param>
    /// <param name="node"></param>
    /// <param name="childNodeList"></param>
    private static void BuildChildNodes(List<TNode> totalNodes, TNode node, List<TNode> childNodeList)
    {
        var nodeSubList = new List<TNode>();
        totalNodes.ForEach(u =>
        {
            if (u.GetParentId().Equals(node.GetId()))
                nodeSubList.Add(u);
        });
        nodeSubList.ForEach(u => BuildChildNodes(totalNodes, u, []));
        childNodeList.AddRange(nodeSubList);
        node.SetChildren(childNodeList);
    }
}