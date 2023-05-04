using Findx.Serialization;

namespace Findx.WebSocketCore;

/// <summary>
/// </summary>
public class WebSocketSerializer : IWebSocketSerializer
{
    private readonly ISerializer _serializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serializer"></param>
    public WebSocketSerializer(ISerializer serializer)
    {
        _serializer = serializer;
    }

    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T item)
    {
        return _serializer.Serialize(item);
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(byte[] serializedObject)
    {
        return _serializer.Deserialize<T>(serializedObject);
    }
}