using System;
using System.Collections.Generic;

namespace Findx.EventBus
{
    [Serializable]
    public class TransportMessage
    {
        public TransportMessage(IDictionary<string, string> headers, byte[] body)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body;
        }

        public IDictionary<string, string> Headers { get; }

        public byte[] Body { get; }

        public string GetId()
        {
            return Headers.TryGetValue(EventBus.Headers.MessageId, out var value) ? value : null;
        }

        public string GetEventName()
        {
            return Headers.TryGetValue(EventBus.Headers.EventName, out var value) ? value : null;
        }

        public string GetTime()
        {
            return Headers.TryGetValue(EventBus.Headers.SentTime, out var value) ? value : null;
        }

        public string GetCorrelationId()
        {
            return Headers.TryGetValue(EventBus.Headers.CorrelationId, out var value) ? value : null;
        }
    }
}
