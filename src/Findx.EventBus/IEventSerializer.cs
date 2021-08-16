using System;

namespace Findx.EventBus
{
    public interface IEventSerializer
    {
        string Serialize<T>(T obj);
        object Deserialize(string content, Type type);
    }
}
