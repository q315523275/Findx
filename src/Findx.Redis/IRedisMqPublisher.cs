using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public interface IRedisMqPublisher
    {
        string RedisClientName { get; }
        void Publish<T>(T obj, string queue);
        Task PublishAsync<T>(T obj, string queue, CancellationToken cancellationToken = default);
    }
}
