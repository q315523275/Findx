﻿using System;
using Findx.Serialization;

namespace Findx.RabbitMQ;

public class DefaultRabbitMqSerializer : IRabbitMqSerializer
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="jsonSerializer"></param>
    public DefaultRabbitMqSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public object Deserialize(string value, Type type)
    {
        return _jsonSerializer.Deserialize(value, type);
    }

    public string Serialize(object obj)
    {
        return _jsonSerializer.Serialize(obj);
    }
}