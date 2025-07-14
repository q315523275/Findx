using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     消息传递服务
/// </summary>
public interface IMessageBroker
{
    /// <summary>
    ///     发送请求并等待响应
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> SendRequestAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     推送异步执行事件
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent;
}