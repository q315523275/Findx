using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    internal class ApplicationEventHandlerWrapperImpl<TRequest, TResponse> : ApplicationEventHandlerWrapper<TResponse> where TRequest : IApplicationEvent<TResponse>
    {
        public override Task<TResponse> Handle(IApplicationEvent<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetService<IApplicationListener<TRequest, TResponse>>();

            Check.NotNull(handler, nameof(handler));

            Task<TResponse> Handler() => handler.Handle((TRequest)request, cancellationToken);

            var messagePipelines = serviceProvider.GetServices<IApplicationEventPipeline<TRequest, TResponse>>();
            if (messagePipelines != null && messagePipelines.Count() > 0)
            {
                return messagePipelines.Reverse()
                                 .Aggregate((ApplicationEventHandlerDelegate<TResponse>)Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, next, cancellationToken))();
            }

            return handler.Handle((TRequest)request, cancellationToken);
        }
    }
    internal abstract class ApplicationEventHandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> Handle(IApplicationEvent<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
