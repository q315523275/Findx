using Findx.Messaging;

namespace Findx.WebHost.Messaging
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
