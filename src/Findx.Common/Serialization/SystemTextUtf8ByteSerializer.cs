using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Findx.Serialization
{
    public class SystemTextUtf8ByteSerializer : ISerializer
    {
        readonly JsonSerializerOptions _options;
        public SystemTextUtf8ByteSerializer()
        {
            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            // _options.Converters.Add(new DateTimeConverter());
        }

        public SystemTextUtf8ByteSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, _options);
        }

        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        }
    }
}
