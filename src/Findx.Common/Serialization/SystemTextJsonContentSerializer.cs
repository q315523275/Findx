using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Findx.Serialization
{
    public class SystemTextJsonContentSerializer : IJsonSerializer
    {
        readonly JsonSerializerOptions jsonSerializerOptions;
        public SystemTextJsonContentSerializer() : this(new JsonSerializerOptions())
        {
            jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.Converters.Add(new DateTimeConverter());
            // jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            // jsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
            // jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public SystemTextJsonContentSerializer(JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        public string Serialize(object obj, bool camelCase = true, bool indented = false)
        {
            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }

        public T Deserialize<T>(string json, bool camelCase = true)
        {
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }

        public object Deserialize(string json, Type type, bool camelCase = true)
        {
            return JsonSerializer.Deserialize(json, type, jsonSerializerOptions);
        }
    }
}
