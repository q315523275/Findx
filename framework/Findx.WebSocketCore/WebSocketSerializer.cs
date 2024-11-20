using System;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Findx.WebSocketCore;

/// <summary>
/// </summary>
public class WebSocketSerializer : IWebSocketSerializer
{
    private readonly IOptions<JsonOptions> _options;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="options"></param>
    public WebSocketSerializer(IOptions<JsonOptions> options)
    {
        _options = options;
    }

    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T item)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item, _options.Value.SerializerOptions);
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(byte[] serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject, _options.Value.SerializerOptions);
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(Span<byte> serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject, _options.Value.SerializerOptions);
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Deserialize<T>(Stream serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject, _options.Value.SerializerOptions);
    }
}