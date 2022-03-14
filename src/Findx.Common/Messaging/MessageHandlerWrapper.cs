using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    internal class MessageHandlerWrapperImpl<TRequest, TResponse> : MessageHandlerWrapper<TResponse> where TRequest : IMessageRequest<TResponse>
    {
        public override Task<TResponse> Handle(IMessageRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetService<IMessageHandler<TRequest, TResponse>>();

            Check.NotNull(handler, nameof(handler));

            var messagePipelines = serviceProvider.GetServices<IMessagePipeline<TRequest, TResponse>>();
            if (messagePipelines != null && messagePipelines.Count() > 0)
            {
                Task<TResponse> Handler() => handler.Handle((TRequest)request, cancellationToken);

                return messagePipelines.Reverse()
                                       .Aggregate((MessageHandlerDelegate<TResponse>)Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, next, cancellationToken))();
            }

            return handler.Handle((TRequest)request, cancellationToken);
        }
    }
    internal abstract class MessageHandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> Handle(IMessageRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
