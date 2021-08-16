using System;
using System.Collections.Generic;

namespace Findx.EventBus
{
    public interface IEventSubscribeManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddDynamicSubscribe<TH>(string eventName)
           where TH : IDynamicEventHandler;

        void AddSubscribe<T, TH>()
           where T : IntegrationEvent
           where TH : IEventHandler<T>;

        void RemoveSubscribe<T, TH>()
             where TH : IEventHandler<T>
             where T : IntegrationEvent;
        void RemoveDynamicSubscribe<TH>(string eventName)
            where TH : IDynamicEventHandler;

        bool HasSubscribeForEvent<T>() where T : IntegrationEvent;
        bool HasSubscribeForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscribeInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscribeInfo> GetHandlersForEvent(string eventName);
        string GetEventKey<T>();
    }
}