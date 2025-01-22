namespace Findx.Redis;

/// <summary>
///     Redis序列化器
/// </summary>
public interface IRedisSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    string Serialize<T>(T item);

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="str"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(string str);
}