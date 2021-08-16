using Findx.Extensions;
using System;

namespace Findx.EventBus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventConsumerAttribute : Attribute
    {
        public string HandlerName { get; set; }
        public int PrefetchCount { get; set; }
        public EventConsumerAttribute(string handlerName)
        {
            Check.NotNullOrWhiteSpace(handlerName, nameof(handlerName));
            HandlerName = handlerName;
        }
        public EventConsumerAttribute(string handlerName, int prefetchCount = 1)
        {
            Check.NotNullOrWhiteSpace(handlerName, nameof(handlerName));
            HandlerName = handlerName;
            PrefetchCount = prefetchCount;
        }

        public static (string, int) GetHandlerParameterOrDefault(Type eventHandlerType)
        {
            Check.NotNull(eventHandlerType, nameof(eventHandlerType));

            var attribute = eventHandlerType.GetAttribute<EventConsumerAttribute>();

            return (attribute?.HandlerName, attribute?.PrefetchCount ?? 1);
        }
    }
}
