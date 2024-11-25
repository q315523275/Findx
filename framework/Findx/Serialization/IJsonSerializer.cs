namespace Findx.Serialization;

/// <summary>
///     JSON序列化处理器
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    ///     对象序列为json
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="camelCase">小驼峰</param>
    /// <param name="indented">整齐打印</param>
    /// <returns></returns>
    string Serialize(object obj, bool camelCase = false, bool indented = false);

    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="camelCase">小驼峰</param>
    /// <returns></returns>
    T Deserialize<T>(string json, bool camelCase = true);
    
    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="camelCase">小驼峰</param>
    /// <returns></returns>
    T Deserialize<T>(ReadOnlySpan<char> json, bool camelCase = true);

    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="camelCase">小驼峰</param>
    /// <returns></returns>
    object Deserialize(string json, Type type, bool camelCase = true);
    
    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="camelCase">小驼峰</param>
    /// <returns></returns>
    object Deserialize(ReadOnlySpan<char> json, Type type, bool camelCase = true);
}