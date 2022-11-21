﻿using Findx.Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{

    public class LocalEventDispatcher : IEventDispatcher
    {
        private readonly IEventSender _sender;
        private readonly IEventExecutor _executor;
        public LocalEventDispatcher(IEventSender sender, IEventExecutor executor)
        {
            _sender = sender;
            _executor = executor;
        }

        public Task EnqueueToExecuteAsync(EventMediumMessage message, CancellationToken cancellationToken = default)
        {
            return _executor.ExecuteAsync(message, cancellationToken);
        }

        public void EnqueueToPublish(EventMediumMessage message)
        {
            var headers = new Dictionary<string, string>
            {
                { Headers.MessageId, message.EventId },
                { Headers.EventName, message.EventName },
                { Headers.SentTime, message.CreateAt.ToString(CultureInfo.InvariantCulture) }
            };

            var transportMessage = new TransportMessage(headers, message.Content.ToBytes());

            _sender.Send(transportMessage);
        }

        public Task EnqueueToPublishAsync(EventMediumMessage message, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                { Headers.MessageId, message.EventId },
                { Headers.EventName, message.EventName },
                { Headers.SentTime, message.CreateAt.ToString(CultureInfo.InvariantCulture) }
            };

            var transportMessage = new TransportMessage(headers, message.Content.ToBytes());

            return _sender.SendAsync(transportMessage, cancellationToken);
        }
    }
}
