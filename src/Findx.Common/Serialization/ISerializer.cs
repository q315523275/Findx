namespace Findx.Serialization;

/// <summary>
///     对象序列化
/// </summary>
public interface ISerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">对象</param>
    byte[] Serialize<T>(T obj);

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="bytes">字节数组</param>
    T Deserialize<T>(byte[] bytes);
}