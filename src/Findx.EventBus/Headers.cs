namespace Findx.EventBus
{
    public static class Headers
    {
        public const string MessageId = "event-id";
        public const string SentTime = "sent-time";
        public const string EventName = "event-name";
        public const string Exception = "exception";
        public const string CorrelationId = "trace-id";
    }
}
