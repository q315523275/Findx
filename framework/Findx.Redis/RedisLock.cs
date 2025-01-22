using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Locks;

namespace Findx.Redis;

public class RedisLock : LockBase, IServiceNameAware
{
    private readonly IRedisClient _redisClient;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="redisClientProvider"></param>
    public RedisLock(IRedisClientProvider redisClientProvider)
    {
        _redisClient = redisClientProvider.CreateClient();
    }

    protected override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
    {
        return await _redisClient.LockAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires, cancellationToken);
    }

    public override async Task ReleaseAsync(string resource, string lockId, CancellationToken cancellationToken = default)
    {
        await _redisClient.LockReleaseAsync(GenerateNewLockKey(resource), lockId, cancellationToken);
    }

    public override async Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
    {
        if (await _redisClient.StringGetAsync(GenerateNewLockKey(resource), cancellationToken) == lockId)
            await _redisClient.ExpireAsync(GenerateNewLockKey(resource), timeUntilExpires, cancellationToken);
    }

    public string Name => "DistributedLock.Redis";
}