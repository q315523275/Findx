namespace Findx.Serialization;

/// <summary>
///     Json序列化与反序列化
/// </summary>
public class SystemTextJsonSerializer : IJsonSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="camelCase"></param>
    /// <param name="indented"></param>
    /// <returns></returns>
    public string Serialize(object obj, bool camelCase = false, bool indented = false)
    {
        return JsonSerializer.Serialize(obj, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase, indented));
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(string json, bool camelCase = false)
    {
        return JsonSerializer.Deserialize<T>(json, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(ReadOnlySpan<char> json, bool camelCase = true)
    {
        return JsonSerializer.Deserialize<T>(json, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    public object Deserialize(string json, Type type, bool camelCase = true)
    {
        return JsonSerializer.Deserialize(json, type, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    public object Deserialize(ReadOnlySpan<char> json, Type type, bool camelCase = true)
    {
        return JsonSerializer.Deserialize(json, type, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }
}