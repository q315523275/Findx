namespace Findx.Data;

/// <summary>
///     树形节点
/// </summary>
public interface ITreeNode<out TKey> where TKey : struct
{
    /// <summary>
    ///     获取节点id
    /// </summary>
    /// <returns></returns>
    TKey GetId();

    /// <summary>
    ///     获取节点父id
    /// </summary>
    /// <returns></returns>
    TKey GetPId();

    /// <summary>
    ///     设置Children
    /// </summary>
    /// <param name="children"></param>
    void SetChildren(IList children);
}