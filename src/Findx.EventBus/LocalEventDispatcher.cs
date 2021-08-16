using Findx.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{

    public class LocalEventDispatcher : IEventDispatcher
    {
        private readonly IEventSender _sender;
        private readonly IEventExecuter _executer;
        public LocalEventDispatcher(IEventSender sender, IEventExecuter executer)
        {
            _sender = sender;
            _executer = executer;
        }

        public Task EnqueueToExecuteAsync(EventMediumMessage message, CancellationToken cancellationToken = default)
        {
            return _executer.ExecuteAsync(message, cancellationToken);
        }

        public void EnqueueToPublish(EventMediumMessage message)
        {
            var headers = new Dictionary<string, string>
            {
                { Headers.MessageId, message.EventId },
                { Headers.EventName, message.EventName },
                { Headers.SentTime, message.CreateAt.ToString() }
            };

            var transportMessage = new TransportMessage(headers, message.Content.GetBytes());

            _sender.Send(transportMessage);
        }

        public Task EnqueueToPublishAsync(EventMediumMessage message, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                { Headers.MessageId, message.EventId },
                { Headers.EventName, message.EventName },
                { Headers.SentTime, message.CreateAt.ToString() }
            };

            var transportMessage = new TransportMessage(headers, message.Content.GetBytes());

            return _sender.SendAsync(transportMessage, cancellationToken);
        }
    }
}
