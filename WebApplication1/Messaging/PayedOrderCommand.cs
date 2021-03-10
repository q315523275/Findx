using Findx.Messaging;

namespace WebApplication1.Messaging
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
