using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     委托消息
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <returns></returns>
public delegate Task<TResponse> MessageHandlerDelegate<TResponse>();

/// <summary>
///     消息管理
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IMessagePipeline<in TRequest, TResponse>
{
    /// <summary>
    ///     处理消息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> HandleAsync(TRequest request, MessageHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}