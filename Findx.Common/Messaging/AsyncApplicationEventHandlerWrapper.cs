using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Messaging
{
    internal abstract class AsyncApplicationEventHandlerWrapper
    {
        public abstract Task Handle(IAsyncApplicationEvent applicationEvent, CancellationToken cancellationToken, IServiceProvider serviceProvider,
                                    Func<IEnumerable<Func<IAsyncApplicationEvent, CancellationToken, Task>>, IAsyncApplicationEvent, CancellationToken, Task> publish);
    }
    internal class AsyncApplicationEventHandlerWrapperImpl<TMessage> : AsyncApplicationEventHandlerWrapper
        where TMessage : IAsyncApplicationEvent
    {
        public override Task Handle(IAsyncApplicationEvent applicationEvent, CancellationToken cancellationToken, IServiceProvider serviceProvider,
                                    Func<IEnumerable<Func<IAsyncApplicationEvent, CancellationToken, Task>>, IAsyncApplicationEvent, CancellationToken, Task> publish)
        {
            var handlers = serviceProvider.GetServices<IAsyncApplicationListener<TMessage>>();
            if (handlers == null || handlers.Count() == 0)
                return Task.CompletedTask;

            var coreHandlers = handlers.Select(x => new Func<IAsyncApplicationEvent, CancellationToken, Task>((theApplicationEvent, theToken) => x.Handle((TMessage)theApplicationEvent, theToken)));

            return publish(coreHandlers, applicationEvent, cancellationToken);
        }
    }
}
