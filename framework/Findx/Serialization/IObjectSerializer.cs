namespace Findx.Serialization;

/// <summary>
///     对象序列化
/// </summary>
public interface IObjectSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">对象</param>
    /// <param name="camelCase">小驼峰</param>
    /// <param name="indented">整齐打印</param>
    byte[] Serialize<T>(T obj, bool camelCase = false, bool indented = false);

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="bytes">字节数组</param>
    /// <param name="camelCase">小驼峰</param>
    T Deserialize<T>(byte[] bytes, bool camelCase = false);
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="bytes">字节数组</param>
    /// <param name="camelCase">小驼峰</param>
    T Deserialize<T>(ReadOnlySpan<byte> bytes, bool camelCase = false);
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="stream">字节数组</param>
    /// <param name="camelCase">小驼峰</param>
    T Deserialize<T>(Stream stream, bool camelCase = false);
}