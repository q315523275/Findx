using System.IO;
using System.Text;
using System.Text.Json;
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
                return Encoding.Default.GetString(message.Memory.Span);

            case ResponseMessageType.Stream:
                if (message.Stream != null)
                {
                    using var reader = new StreamReader(message.Stream, Encoding.Default);
                    return await reader.ReadToEndAsync();
                };
                break;
        }
        return default;
    }
}