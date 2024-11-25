namespace Findx.Serialization;

/// <summary>
///     序列化
/// </summary>
public class SystemTextByteSerializer : IObjectSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="camelCase"></param>
    /// <param name="indented"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T obj, bool camelCase = false, bool indented = false)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase, indented));
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(byte[] bytes, bool camelCase = false)
    {
        return JsonSerializer.Deserialize<T>(bytes, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(Stream stream, bool camelCase = false)
    {
        return JsonSerializer.Deserialize<T>(stream, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(ReadOnlySpan<byte> bytes, bool camelCase = false)
    {
        return JsonSerializer.Deserialize<T>(bytes, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(camelCase));
    }
}