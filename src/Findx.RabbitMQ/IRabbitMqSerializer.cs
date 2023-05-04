using System;

namespace Findx.RabbitMQ
{
    public interface IRabbitMqSerializer
    {
        string Serialize(object obj);

        object Deserialize(string value, Type type);
    }
}