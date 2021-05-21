using Findx.EventBus.Abstractions;
using Findx.EventBus.Events;
using System.Threading.Tasks;

namespace Findx.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus
    {
        private readonly IEventPublisher _publisher;
        private readonly IEventSubscriber _subscriber;

        public EventBusRabbitMQ(IEventPublisher publisher, IEventSubscriber subscriber)
        {
            _publisher = publisher;
            _subscriber = subscriber;
        }

        public void Publish(IntegrationEvent @event)
        {
            _publisher.Publish(@event);
        }

        public Task PublishAsync(IntegrationEvent @event)
        {
            return _publisher.PublishAsync(@event);
        }

        public void StartConsuming()
        {
            _subscriber.StartConsuming();
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>
        {
            _subscriber.Subscribe<T, TH>();
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            _subscriber.SubscribeDynamic<TH>(eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>
        {
            _subscriber.Unsubscribe<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            _subscriber.UnsubscribeDynamic<TH>(eventName);
        }
    }
}
