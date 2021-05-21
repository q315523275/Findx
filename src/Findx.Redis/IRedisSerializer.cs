namespace Findx.Redis
{
    public interface IRedisSerializer
    {
        string Serialize<T>(T item);
        T Deserialize<T>(string serializedObject);
    }
}
