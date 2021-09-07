using Findx.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Findx.EventBus
{
    public partial class InMemoryEventSubscribeManager : IEventSubscribeManager, ISingletonDependency
    {
        private readonly IDictionary<string, List<SubscribeInfo>> _handlers;
        private readonly IDictionary<string, Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public InMemoryEventSubscribeManager()
        {
            _handlers = new ConcurrentDictionary<string, List<SubscribeInfo>>();
            _eventTypes = new ConcurrentDictionary<string, Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddDynamicSubscribe<TH>(string eventName)
            where TH : IDynamicEventHandler
        {
            DoAddSubscribe(typeof(TH), eventName, isDynamic: true);
        }

        public void AddSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscribe(typeof(TH), eventName, isDynamic: false);

            if (!_eventTypes.ContainsKey(eventName))
            {
                _eventTypes.Add(eventName, typeof(T));
            }
        }

        private void DoAddSubscribe(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscribeForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscribeInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            if (isDynamic)
            {
                _handlers[eventName].Add(SubscribeInfo.Dynamic(handlerType));
            }
            else
            {
                _handlers[eventName].Add(SubscribeInfo.Typed(handlerType));
            }
        }

        public void RemoveDynamicSubscribe<TH>(string eventName)
            where TH : IDynamicEventHandler
        {
            var handlerToRemove = FindDynamicSubscribeToRemove<TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        public void RemoveSubscribe<T, TH>()
            where TH : IEventHandler<T>
            where T : IntegrationEvent
        {
            var handlerToRemove = FindSubscribeToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }

        private void DoRemoveHandler(string eventName, SubscribeInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    if (_eventTypes.ContainsKey(eventName))
                    {
                        _eventTypes.Remove(eventName);
                    }
                    RaiseOnEventRemoved(eventName);
                }

            }
        }

        public IEnumerable<SubscribeInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscribeInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscribeInfo FindDynamicSubscribeToRemove<TH>(string eventName)
            where TH : IDynamicEventHandler
        {
            return DoFindSubscribeToRemove(eventName, typeof(TH));
        }

        private SubscribeInfo FindSubscribeToRemove<T, TH>()
             where T : IntegrationEvent
             where TH : IEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscribeToRemove(eventName, typeof(TH));
        }

        private SubscribeInfo DoFindSubscribeToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscribeForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);

        }

        public bool HasSubscribeForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscribeForEvent(key);
        }

        public bool HasSubscribeForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes[eventName];

        public string GetEventKey<T>()
        {
            var eventType = typeof(T);
            return EventNameAttribute.GetNameOrDefault(eventType);
        }
    }
}
