using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace Findx.Messaging
{
    internal abstract class MessageNotifyHandlerWrapper
    {
        public abstract Task Handle(IMessageNotify message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    }

    internal class MessageNotifyHandlerWrapperImpl<TMessage> : MessageNotifyHandlerWrapper where TMessage : IMessageNotify
    {
        public override async Task Handle(IMessageNotify message, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var handlers = serviceProvider.GetServices<IMessageNotifyHandler<TMessage>>();
            foreach (var handler in handlers)
            {
                await handler.Handle((TMessage)message, cancellationToken);
            }
        }
    }
}
