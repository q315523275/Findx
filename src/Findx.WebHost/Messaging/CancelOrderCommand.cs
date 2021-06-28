using Findx.Messaging;

namespace Findx.WebHost.Messaging
{
    public class CancelOrderCommand : IMessageRequest<bool>
    {
        public CancelOrderCommand()
        {
        }

        public CancelOrderCommand(long orderId)
        {
            OrderId = orderId;
        }

        public long OrderId { get; private set; }
    }
}
