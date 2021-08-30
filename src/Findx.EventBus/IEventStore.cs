namespace Findx.EventBus
{
    /// <summary>
    /// 事件存储
    /// </summary>
    public interface IEventStore
    {
        EventMediumMessage SavePublishedEvent(IntegrationEvent integrationEvent, object transaction = null);

        EventMediumMessage SaveReceivedEvent(string eventId, string eventName, string content);

        void SaveReceivedExceptionEvent(string eventId, string eventName, string content);
    }
}