using Findx.EventBus.Events;
using System.Threading.Tasks;

namespace Findx.EventBus.Abstractions
{
    public interface IEventHandler<in TIntegrationEvent> : IEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent @event);
    }

    public interface IEventHandler
    {
    }
}
