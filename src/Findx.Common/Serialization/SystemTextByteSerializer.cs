using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Findx.Serialization
{
    public class SystemTextByteSerializer : ISerializer
    {
        readonly JsonSerializerOptions jsonSerializerOptions;
        public SystemTextByteSerializer() : this(new JsonSerializerOptions())
        {
            jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            // jsonSerializerOptions.Converters.Add(new DateTimeConverter());
            // jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public SystemTextByteSerializer(JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, jsonSerializerOptions);
        }

        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, jsonSerializerOptions);
        }
    }
}
