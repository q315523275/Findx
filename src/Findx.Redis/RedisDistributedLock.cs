using Findx.Locks;
using System;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public class RedisDistributedLock : LockBase
    {
        private readonly IRedisClient _redisClient;

        public override LockType LockType => LockType.Distributed;

        public RedisDistributedLock(IRedisClientProvider redisClientProvider)
        {
            _redisClient = redisClientProvider.CreateClient();
        }

        public override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            return await _redisClient.LockTakeAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires.Value);
        }

        public override async Task ReleaseAsync(string resource, string lockId)
        {
            await _redisClient.LockReleaseAsync(GenerateNewLockKey(resource), lockId);
        }

        public override async Task RenewAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            if (await _redisClient.StringGetAsync<string>(GenerateNewLockKey(resource)) == lockId)
                await _redisClient.ExpireAsync(GenerateNewLockKey(resource), DateTime.Now.Add(timeUntilExpires.Value));
        }
    }
}
