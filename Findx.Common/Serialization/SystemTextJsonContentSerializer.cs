using Findx.Extensions;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            // jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.Converters.Add(new DateTimeConverter());
            // jsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
            // jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
    public class DateTimeNullableConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().IsNullOrWhiteSpace() ? default(DateTime?) : DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
