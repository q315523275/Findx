using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Messaging
{
    public class MessageSender : IMessageSender
    {
        private static readonly IDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();

        private readonly IServiceProvider _serviceProvider;

        public MessageSender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));

            var messageType = message.GetType();

            var handler = (MessageHandlerWrapper<TResponse>)_messageHandlers.GetOrAdd(messageType,
                         t => Activator.CreateInstance(typeof(MessageHandlerWrapperImpl<,>).MakeGenericType(messageType, typeof(TResponse))));

            Check.NotNull(handler, nameof(handler));

            return handler.Handle(message, _serviceProvider, cancellationToken);
        }
    }
}
