namespace Findx.Serialization;

/// <summary>
///     序列化扩展
/// </summary>
public static class SerializerExtensions
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="serializer"></param>
	/// <param name="model"></param>
	/// <returns></returns>
	public static byte[] SerializeToBytes<T>(this IObjectSerializer serializer, T model)
    {
        return serializer.Serialize(model);
    }
}