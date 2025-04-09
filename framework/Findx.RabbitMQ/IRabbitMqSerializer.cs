using System;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ序列化器服务
/// </summary>
public interface IRabbitMqSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    string Serialize(object obj);

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    object Deserialize(string value, Type type);
}