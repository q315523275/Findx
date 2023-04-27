using Findx.EventBus;

namespace Findx.WebHost.EventBus
{
    public class FindxTestEvent : EventDataBase
    {
        public string Body { set; get; }
    }
}
