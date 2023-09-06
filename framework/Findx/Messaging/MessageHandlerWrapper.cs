using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Messaging;

/// <summary>
///     内部消息执行器包装
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
internal class MessageHandlerWrapperImpl<TRequest, TResponse> : MessageHandlerWrapper<TResponse>
    where TRequest : IMessageRequest<TResponse>
{
    public override Task<TResponse> HandleAsync(IMessageRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IMessageHandler<TRequest, TResponse>>();

        Check.NotNull(handler, nameof(handler));

        var messagePipelines = serviceProvider.GetServices<IMessagePipeline<TRequest, TResponse>>();
        // ReSharper disable once PossibleMultipleEnumeration
        if (!messagePipelines.Any())
            return handler.HandleAsync((TRequest)request, cancellationToken);

        Task<TResponse> Handler()
        {
            return handler.HandleAsync((TRequest)request, cancellationToken);
        }

        // ReSharper disable once PossibleMultipleEnumeration
        return messagePipelines.Reverse().Aggregate((MessageHandlerDelegate<TResponse>)Handler, (next, pipeline) => () => pipeline.HandleAsync((TRequest)request, next, cancellationToken))();
    }
}

/// <summary>
///     内部消息执行器包装
/// </summary>
/// <typeparam name="TResponse"></typeparam>
internal abstract class MessageHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> HandleAsync(IMessageRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}