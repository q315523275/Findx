using Findx.EventBus.Events;
using System.Threading.Tasks;

namespace Findx.EventBus.Abstractions
{
    public interface IEventPublisher
    {
        void Publish(IntegrationEvent @event);
        Task PublishAsync(IntegrationEvent @event);
    }
}
