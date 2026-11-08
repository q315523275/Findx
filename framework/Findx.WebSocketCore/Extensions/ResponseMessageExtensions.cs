using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore.Extensions;

/// <summary>
///     响应消息扩展
/// </summary>
public static class ResponseMessageExtensions
{
    /// <summary>
    ///     响应消息反序列化
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Deserialize<T>(this ResponseMessage message)
    {
        var serializer = ServiceLocator.GetService<IWebSocketSerializer>();
        if (serializer == null)
            throw new InvalidOperationException($"未注册 {nameof(IWebSocketSerializer)}，请在WebSocket组件模块注册完成后再调用消息反序列化扩展。");
        switch (message.ResponseMessageType)
        {
            case ResponseMessageType.Memory:
                return serializer.Deserialize<T>(message.Memory.Span);

            case ResponseMessageType.Stream:
                return serializer.Deserialize<T>(message.Stream);
        }
        
        return default;
    }

    /// <summary>
    ///     获取响应消息文本内容
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> AsTextAsync(this ResponseMessage message, CancellationToken cancellationToken = default)
    {
        switch (message.ResponseMessageType)
        {
            case ResponseMessageType.Memory:
                return Encoding.UTF8.GetString(message.Memory.Span);

            case ResponseMessageType.Stream:
                if (message.Stream != null)
                {
                    var stream = message.Stream;

                    //  已读到末尾的流重置到起点，避免同一消息重复调用 AsTextAsync 时拿到空字符串
                    if (stream.CanSeek && stream.Position >= stream.Length)
                        stream.Position = 0;

                    //  UTF-8为WebSocket文本帧标准编码；detectEncodingFromByteOrderMarks=true 兼容带BOM的内容（BOM会被自动跳过，不会残留 \uFEFF）；
                    //  leaveOpen=true，StreamReader不释放调用方持有的Stream
                    using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
                    #if NET8_0_OR_GREATER
                        return await reader.ReadToEndAsync(cancellationToken);
                    #else
                        return await reader.ReadToEndAsync();
                    #endif
                };
                break;
        }
        return string.Empty;
    }
}