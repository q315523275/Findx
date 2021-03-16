using Findx.Messaging;

namespace Findx.WebHost.Messaging
{
    public class PayedOrderCommand : IAsyncApplicationEvent
    {
        public PayedOrderCommand()
        {
        }

        public PayedOrderCommand(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; private set; }
    }
}
