using System.Threading.Tasks;
using Findx.Pipelines;

namespace Findx.Messaging;

internal abstract class ApplicationEventHandlerWrapper
{
    public abstract Task HandleAsync(IApplicationEvent message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}

internal class ApplicationEventHandlerWrapperImpl<TEvent> : ApplicationEventHandlerWrapper where TEvent : IApplicationEvent
{
    public override async Task HandleAsync(IApplicationEvent message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var applicationEvent = (TEvent)message;
        var handlers = serviceProvider.GetServices<IApplicationEventHandler<TEvent>>();
        var pipelines = serviceProvider.GetServices<IApplicationEventPipeline<TEvent>>();
        // ReSharper disable once PossibleMultipleEnumeration
        if (!pipelines.Any())
        {
            await ExecuteHandlersAsync(handlers, applicationEvent, cancellationToken);
        }
        else
        {
            Task Handler() { return ExecuteHandlersAsync(handlers, applicationEvent, cancellationToken); }
            // ReSharper disable once PossibleMultipleEnumeration
            pipelines.Reverse().Aggregate((PipelineInvokeDelegate)Handler, (next, pipeline) => () => pipeline.InvokeAsync(applicationEvent, next, cancellationToken))();
        }
    }

    private static async Task ExecuteHandlersAsync(IEnumerable<IApplicationEventHandler<TEvent>> handlers, TEvent applicationEvent, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(applicationEvent, cancellationToken);
        }
    }
}