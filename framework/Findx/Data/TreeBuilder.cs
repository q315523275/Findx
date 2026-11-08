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
        if (nodes == null || nodes.Count == 0) yield break;

        //  一次性索引父->子映射，避免 O(n²) 全表扫描
        var childrenMap = new Dictionary<TKey, List<TNode>>();
        foreach (var node in nodes)
        {
            var parentId = node.GetParentId();
            if (!childrenMap.TryGetValue(parentId, out var list))
            {
                list = [];
                childrenMap[parentId] = list;
            }
            list.Add(node);
        }

        //  防环：记录已挂接的节点，父子环（A->B->A）与自引用直接跳过，避免 StackOverflow 进程崩溃
        var visited = new HashSet<TKey>();
        foreach (var node in nodes)
        {
            if (visited.Add(node.GetId()))
                BuildChildNodes(childrenMap, node, visited);
        }

        foreach (var item in nodes.Where(item => rootParentId.Equals(item.GetParentId())))
        {
            yield return item;
        }
    }

    /// <summary>
    ///     构造子节点集合
    /// </summary>
    /// <param name="childrenMap">父->子 索引</param>
    /// <param name="node"></param>
    /// <param name="visited">已挂接节点集合（防环）</param>
    private static void BuildChildNodes(Dictionary<TKey, List<TNode>> childrenMap, TNode node, HashSet<TKey> visited)
    {
        if (!childrenMap.TryGetValue(node.GetId(), out var children))
        {
            node.SetChildren(new List<TNode>());
            return;
        }

        var childList = new List<TNode>();
        foreach (var child in children)
        {
            // 环检测：该子节点已在访问链上（如 A->B->A），跳过避免无限递归
            if (!visited.Add(child.GetId())) continue;

            BuildChildNodes(childrenMap, child, visited);
            childList.Add(child);
        }
        node.SetChildren(childList);
    }
}