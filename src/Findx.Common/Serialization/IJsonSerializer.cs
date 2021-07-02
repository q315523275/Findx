using System;

namespace Findx.Serialization
{
    /// <summary>
    /// JSON序列化处理器
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// 对象序列为Jjson
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="camelCase"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        string Serialize(object obj, bool camelCase = true, bool indented = false);

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        T Deserialize<T>(string json, bool camelCase = true);

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        object Deserialize(string json, Type type, bool camelCase = true);
    }
}
