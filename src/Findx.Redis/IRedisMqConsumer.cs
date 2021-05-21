using System;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public interface IRedisMqConsumer
    {
        string RedisClientName { get; }

        void OnMessageReceived(Func<string, string, Task> callback);

        void StartConsuming();
    }
}
