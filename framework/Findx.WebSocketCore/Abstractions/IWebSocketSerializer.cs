using System;
using System.IO;

namespace Findx.WebSocketCore.Abstractions;

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
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(ReadOnlySpan<byte> serializedObject);
    
    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(Stream serializedObject);
}
