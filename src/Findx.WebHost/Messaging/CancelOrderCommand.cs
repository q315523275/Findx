using Findx.Messaging;

namespace Findx.WebHost.Messaging
{
    public class CancelOrderCommand : IMessageRequest<bool>
    {
        public CancelOrderCommand()
        {
        }

        public CancelOrderCommand(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; private set; }
    }
}
