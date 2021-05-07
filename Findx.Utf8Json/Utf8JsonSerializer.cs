using Findx.Serialization;
using Utf8Json;

namespace Findx.Utf8Json
{
    public class Utf8JsonSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }

        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
