using Findx.EventBus.Events;

namespace Findx.WebHost.EventBus
{
    public class FindxTestEvent : IntegrationEvent
    {
        public string Body { set; get; }
    }
}
