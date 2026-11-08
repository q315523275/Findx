using Findx.Data;

namespace Findx.UnitTests;

/// <summary>
///     TreeBuilder 回归测试（修复：O(n²) 全表扫描 → 索引化；父子环/自引用防 StackOverflow）
/// </summary>
public class TreeBuilderTests
{
    private sealed class Node : ITreeNode<int>
    {
        public Node(int id, int parentId)
        {
            _id = id;
            _parentId = parentId;
        }

        private readonly int _id;
        private readonly int _parentId;
        public List<Node> Children { get; private set; } = [];

        public int GetId() => _id;
        public int GetParentId() => _parentId;
        public void SetChildren(System.Collections.IList children) => Children = children.Cast<Node>().ToList();
    }

    [Fact]
    public void Build_NormalTree_AssemblesHierarchy()
    {
        var nodes = new List<Node>
        {
            new(1, 0), // 根
            new(2, 1),
            new(3, 1),
            new(4, 2)
        };

        var roots = new TreeBuilder<Node, int>().Build(nodes, 0).ToList();

        Assert.Single(roots);
        Assert.Equal(1, roots[0].GetId());
        Assert.Equal(2, roots[0].Children.Count); // 根下挂 2、3
        var node2 = roots[0].Children.Single(c => c.GetId() == 2);
        Assert.Single(node2.Children); // 2 下挂 4
        Assert.Equal(4, node2.Children[0].GetId());
    }

    [Fact]
    public void Build_WithCycle_DoesNotStackOverflow()
    {
        // 环：2 的父是 1，1 的父是 2（A->B->A）
        var nodes = new List<Node>
        {
            new(1, 2),
            new(2, 1)
        };

        // 修复前：递归无限循环 → StackOverflow；修复后：visited 跳过环
        var roots = new TreeBuilder<Node, int>().Build(nodes, 0).ToList();
        Assert.Empty(roots);
    }

    [Fact]
    public void Build_WithSelfReference_DoesNotStackOverflow()
    {
        var nodes = new List<Node> { new(1, 1) };

        var roots = new TreeBuilder<Node, int>().Build(nodes, 0).ToList();
        Assert.Empty(roots);
    }
}
