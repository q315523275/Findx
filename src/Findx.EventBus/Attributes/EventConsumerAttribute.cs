using System;

namespace Findx.EventBus.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventConsumerAttribute : Attribute
    {
        public string QueueName
        {
            get { return _queueName; }
        }
        public ushort PrefetchCount
        {
            get { return _prefetchCount; }
        }

        private string _queueName { get; set; }
        private ushort _prefetchCount { get; set; }
        public EventConsumerAttribute(string queueName)
        {
            Check.NotNullOrWhiteSpace(queueName, nameof(queueName));
            _queueName = queueName;
        }
        public EventConsumerAttribute(string queueName, ushort prefetchCount = 1)
        {
            Check.NotNullOrWhiteSpace(queueName, nameof(queueName));
            _queueName = queueName;
            _prefetchCount = prefetchCount;
        }
    }
}
