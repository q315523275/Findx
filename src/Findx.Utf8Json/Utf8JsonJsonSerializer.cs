using Findx.Serialization;
using System;
using Utf8Json;

namespace Findx.Utf8Json
{
    public class Utf8JsonJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public object Deserialize(string json, Type type)
        {
            return JsonSerializer.NonGeneric.Deserialize(type, json);
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.ToJsonString(obj);
        }
    }
}
