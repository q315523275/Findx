using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Findx.Serialization
{
    public class SystemTextJsonStringSerializer : IJsonSerializer
    {
        readonly JsonSerializerOptions _options;
        public SystemTextJsonStringSerializer()
        {
            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
            };
        }

        public SystemTextJsonStringSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, _options);
        }
    }
}
