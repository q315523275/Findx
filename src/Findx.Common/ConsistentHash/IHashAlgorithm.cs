namespace Findx.ConsistentHash;

/// <summary>
///     哈希算法接口
/// </summary>
public interface IHashAlgorithm
{
    /// <summary>
    ///     Hash值
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    int Hash(string item);
}