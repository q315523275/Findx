namespace Findx;

/// <summary>
///     泛型一致性hash
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConsistentHash<T>
{
    private readonly IHashAlgorithm _hashAlgorithm;
    private readonly SortedDictionary<int, T> _ring = new();
    private int[] _nodeKeysInRing;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="hashAlgorithm">哈希算法</param>
    public ConsistentHash(IHashAlgorithm hashAlgorithm)
    {
        _hashAlgorithm = hashAlgorithm;
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public ConsistentHash() : this(new MurmurHash2HashAlgorithm())
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="hashAlgorithm">哈希算法</param>
    /// <param name="virtualNodeReplicationFactor">虚拟节点复制因子数</param>
    public ConsistentHash(IHashAlgorithm hashAlgorithm, int virtualNodeReplicationFactor)
        : this(hashAlgorithm)
    {
        VirtualNodeReplicationFactor = virtualNodeReplicationFactor;
    }

    /// <summary>
    ///     虚拟节点复制因子数
    /// </summary>
    public int VirtualNodeReplicationFactor { get; } = 1000;

    /// <summary>
    ///     初始化节点集合
    /// </summary>
    /// <param name="nodes"></param>
    public void Initialize(IEnumerable<T> nodes)
    {
        foreach (var node in nodes) AddNode(node);

        _nodeKeysInRing = _ring.Keys.ToArray();
    }

    /// <summary>
    ///     添加节点
    /// </summary>
    /// <param name="node"></param>
    public void Add(T node)
    {
        AddNode(node);
        _nodeKeysInRing = _ring.Keys.ToArray();
    }

    /// <summary>
    ///     删除节点
    /// </summary>
    /// <param name="node"></param>
    public void Remove(T node)
    {
        RemoveNode(node);
        _nodeKeysInRing = _ring.Keys.ToArray();
    }

    /// <summary>
    ///     判断是否包含节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool ContainNode(T node)
    {
        return _ring.ContainsValue(node);
    }


    /// <summary>
    ///     私有添加节点方法
    /// </summary>
    /// <param name="node"></param>
    private void AddNode(T node)
    {
        for (var i = 0; i < VirtualNodeReplicationFactor; i++)
        {
            var hashOfVirtualNode = _hashAlgorithm.Hash(node.GetHashCode().ToString() + i);
            _ring[hashOfVirtualNode] = node;
        }
    }

    private void RemoveNode(T node)
    {
        for (var i = 0; i < VirtualNodeReplicationFactor; i++)
        {
            var hashOfVirtualNode = _hashAlgorithm.Hash(node.GetHashCode().ToString() + i);
            _ring.Remove(hashOfVirtualNode);
        }
    }

    /// <summary>
    ///     获取节点
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public T GetItemNode(string item)
    {
        var hashOfItem = _hashAlgorithm.Hash(item);
        var nearestNodePosition = GetClockwiseNearestNode(_nodeKeysInRing, hashOfItem);
        return _ring[_nodeKeysInRing[nearestNodePosition]];
    }

    /// <summary>
    ///     获取顺时针最近节点
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="hashOfItem"></param>
    /// <returns></returns>
    private int GetClockwiseNearestNode(IReadOnlyList<int> keys, int hashOfItem)
    {
        var begin = 0;
        var end = keys.Count - 1;

        if (keys[end] < hashOfItem || keys[0] > hashOfItem) return 0;

        while (end - begin > 1)
        {
            var mid = (end + begin) / 2;
            if (keys[mid] >= hashOfItem) end = mid;
            else begin = mid;
        }

        return end;
    }

    /// <summary>
    ///     获取节点总数
    /// </summary>
    /// <returns></returns>
    public int GetNodeCount()
    {
        return _ring.Count();
    }
}