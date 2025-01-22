using Findx.Messaging;

namespace Findx.WebHost.EventHandlers;

public class CancelOrderCommand : IMessageRequest<string>
{
    public CancelOrderCommand(string orderId)
    {
        OrderId = orderId;
    }

    public string OrderId { set; get; }
}