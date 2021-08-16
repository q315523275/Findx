using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Findx.EventBus.RabbitMQ
{
    public class RabbitMQAcknowledge
    {
        public RabbitMQAcknowledge(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            Channel = channel;
            EventArgs = eventArgs;
        }

        public IModel Channel { get; }

        public BasicDeliverEventArgs EventArgs { get; }
    }
}
