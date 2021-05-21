using Findx.EventBus.Events;

namespace Findx.EventBus.Abstractions
{
    public interface IEventSubscriber
    {
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>;

        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IEventHandler<T>
            where T : IntegrationEvent;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void StartConsuming();
    }
}
