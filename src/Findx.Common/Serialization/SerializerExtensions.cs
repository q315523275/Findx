using System;
namespace Findx.Serialization
{
	public static class SerializerExtensions
	{
		/// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="model"></param>
        /// <returns></returns>
		public static byte[] SerializeToBytes<T>(this ISerializer serializer, T model)
		{
            return serializer.Serialize(model);
        }
	}
}
