namespace Findx.WebSocketCore;

/// <summary>
///     序列化器
/// </summary>
public interface IWebSocketSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    byte[] Serialize<T>(T item);

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(byte[] serializedObject);
}