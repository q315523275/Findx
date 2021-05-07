using Findx.Serialization;
using System;
using Utf8Json;

namespace Findx.Utf8Json
{
    public class Utf8JsonJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json, bool camelCase = true)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public object Deserialize(string json, Type type, bool camelCase = true)
        {
            return JsonSerializer.NonGeneric.Deserialize(type, json);
        }

        public string Serialize(object obj, bool camelCase = true, bool indented = false)
        {
            return JsonSerializer.ToJsonString(obj);
        }
    }
}
