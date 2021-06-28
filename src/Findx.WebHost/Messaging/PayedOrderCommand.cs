using Findx.Messaging;

namespace Findx.WebHost.Messaging
{
    public class PayedOrderCommand : IMessageNotify
    {
        public PayedOrderCommand()
        {
        }

        public PayedOrderCommand(long orderId)
        {
            OrderId = orderId;
        }

        public long OrderId { get; private set; }
    }
}
