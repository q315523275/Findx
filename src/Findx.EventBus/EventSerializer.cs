using Findx.Serialization;
using System;

namespace Findx.EventBus
{
    public class EventSerializer : IEventSerializer
    {
        private readonly IJsonSerializer _serializer;

        public EventSerializer(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public object Deserialize(string content, Type type)
        {
            return _serializer.Deserialize(content, type);
        }

        public string Serialize<T>(T obj)
        {
            return _serializer.Serialize(obj);
        }
    }
}
