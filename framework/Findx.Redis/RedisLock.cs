using System;
using System.Globalization;
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
        //  Lua 原子续期：仅当锁当前持有者 token 仍匹配时才延长 TTL，
        //  避免 StringGet+Expire 两步非原子导致误续期他人持有的锁。
        var lockKey = GenerateNewLockKey(resource);
        var script = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('pexpire', KEYS[1], ARGV[2]) else return 0 end";
        var milliseconds = ((long)timeUntilExpires.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        await _redisClient.EvalAsync(script, lockKey, [lockId, milliseconds]);
    }

    public string Name => "DistributedLock.Redis";
}