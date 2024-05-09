using Findx.Common;
using Findx.Messaging;

namespace Findx.WebHost.EventHandlers;

public class PayedOrderCommand : IApplicationEvent, IAsync
{
    public PayedOrderCommand()
    {
    }

    public PayedOrderCommand(long orderId)
    {
        OrderId = orderId;
    }

    public long OrderId { get; set; }
}