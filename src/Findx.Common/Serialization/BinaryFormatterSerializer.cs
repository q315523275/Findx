using Findx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
namespace Findx.Serialization
{
    [Dependency(ServiceLifetime.Singleton)]
    public class BinaryFormatterSerializer : ISerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public virtual T Deserialize<T>(byte[] bytes)
        {
            if (typeof(Task).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException(typeof(T).Name);
            }
            using var memoryStream = new MemoryStream(bytes);
            return (T)_binaryFormatter.Deserialize(memoryStream);
        }

        public virtual byte[] Serialize<T>(T obj)
        {
            if (typeof(Task).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException(typeof(T).Name);
            }
            using var memoryStream = new MemoryStream();
            _binaryFormatter.Serialize(memoryStream, obj);
            return memoryStream.ToArray();
        }
    }

}
