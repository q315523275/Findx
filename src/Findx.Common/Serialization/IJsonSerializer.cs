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
    /// <returns></returns>
    string Serialize(object obj);

    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    T Deserialize<T>(string json);

    /// <summary>
    ///     Json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    object Deserialize(string json, Type type);
}