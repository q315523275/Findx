using Findx.Messaging;

namespace WebApplication1.Messaging
{
    public class CancelOrderCommand : IApplicationEvent<bool>
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
