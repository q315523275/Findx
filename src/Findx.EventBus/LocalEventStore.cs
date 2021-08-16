using Findx.Extensions;
using System;

namespace Findx.EventBus
{
    public class LocalEventStore : IEventStore
    {
        private readonly IEventSerializer _serializer;

        public LocalEventStore(IEventSerializer serializer)
        {
            _serializer = serializer;
        }

        public EventMediumMessage SavePublishedEvent(IntegrationEvent integrationEvent, object transaction = null)
        {
            var message = new EventMediumMessage
            {
                Content = _serializer.Serialize(integrationEvent),
                CreateAt = integrationEvent.CreationDate,
                EventId = integrationEvent.Id.ToString(),
                EventName = EventNameAttribute.GetNameOrDefault(integrationEvent.GetType()),
                Status = EventStatus.InProgress
            };
            return message;
        }

        public EventMediumMessage SaveReceivedEvent(string eventId, string eventName, string content)
        {
            var message = new EventMediumMessage
            {
                Content = content,
                CreateAt = DateTime.Now,
                EventId = eventId,
                EventName = eventName,
                Status = EventStatus.InProgress
            };
            return message;
        }

        public void SaveReceivedExceptionEvent(string eventId, string eventName, string content)
        {

        }
    }
}
