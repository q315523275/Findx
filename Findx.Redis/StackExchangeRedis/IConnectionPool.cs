using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    public interface IConnectionPool
    {
        ConnectionMultiplexer Acquire(string connection = null);
        Task<ConnectionMultiplexer> AcquireAsync(string connection = null, CancellationToken cancellationToken = default);
    }
}
