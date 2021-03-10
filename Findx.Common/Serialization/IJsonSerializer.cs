using System;

namespace Findx.Serialization
{
    /// <summary>
    /// JSON序列化处理器
    /// </summary>
    public interface IJsonSerializer
    {
        string Serialize(object obj, bool camelCase = true, bool indented = false);

        T Deserialize<T>(string json, bool camelCase = true);

        object Deserialize(string json, Type type, bool camelCase = true);
    }
}
