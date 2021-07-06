using System;

namespace Findx.RabbitMQ
{
    public interface IRabbitMQSerializer
    {
        string Serialize(object obj);

        object Deserialize(string value, Type type);
    }
}
