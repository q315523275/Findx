namespace Findx.WebSocketCore
{
    public enum MessageType
    {
        Text,
        ConnectionEvent,
        Error
    }

    public class WebSocketMessage
    {
        public MessageType MessageType { get; set; }
        public string Data { get; set; }
    }
}
