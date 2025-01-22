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