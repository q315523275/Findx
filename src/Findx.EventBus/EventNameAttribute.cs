using Findx.Extensions;
using System;
namespace Findx.EventBus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventNameAttribute : Attribute
    {
        public string Name { get; }

        public EventNameAttribute(string name)
        {
            Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        }

        public static string GetNameOrDefault(Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));

            return eventType.GetAttribute<EventNameAttribute>()?.Name ?? eventType.FullName;
        }
    }
}
