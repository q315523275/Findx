using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Messaging
{
    internal abstract class ApplicationEventHandlerWrapper
    {
        public abstract Task Handle(IApplicationEvent message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    }

    internal class ApplicationEventHandlerWrapperImpl<TEvent> : ApplicationEventHandlerWrapper where TEvent : IApplicationEvent
    {
        public override async Task Handle(IApplicationEvent message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var handlers = serviceProvider.GetServices<IApplicationEventHandler<TEvent>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync((TEvent)message, cancellationToken);
            }
        }
    }
}
