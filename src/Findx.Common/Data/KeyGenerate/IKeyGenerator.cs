namespace Findx.Data;

/// <summary>
///     定义TKey类型主键生成器
/// </summary>
public interface IKeyGenerator<out TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     获取一个TKey类型的主键数据
    /// </summary>
    /// <returns></returns>
    TKey Create();
}