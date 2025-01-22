using Findx.Extensions;

namespace Findx.Redis;

public class RedisSerializer : IRedisSerializer
{
    public T Deserialize<T>(string serializedObject)
    {
        return serializedObject.ToObject<T>();
    }

    public string Serialize<T>(T item)
    {
        return item.ToJson();
    }
}