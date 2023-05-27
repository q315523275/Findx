using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     消息处理器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IMessageHandler<in TRequest, TResponse> where TRequest : IMessageRequest<TResponse>
{
    /// <summary>
    ///     处理消息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}