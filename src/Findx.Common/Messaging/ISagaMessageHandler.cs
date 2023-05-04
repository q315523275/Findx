using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     Saga模式消息处理器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ISagaMessageHandler<in TRequest, TResponse> : IMessageHandler<TRequest, TResponse>
    where TRequest : IMessageRequest<TResponse>
{
    /// <summary>
    ///     取消处理
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> CancelAsync(TRequest request, CancellationToken cancellationToken = default);
}