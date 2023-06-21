using System;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Locks;

namespace Findx.Redis
{
    public class RedisDistributedLock : LockBase, IServiceNameAware
    {
        private readonly IRedisClient _redisClient;

        public RedisDistributedLock(IRedisClientProvider redisClientProvider)
        {
            _redisClient = redisClientProvider.CreateClient();
        }

        protected override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan timeUntilExpires)
        {
            return await _redisClient.LockTakeAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires);
        }

        public override async Task ReleaseAsync(string resource, string lockId)
        {
            await _redisClient.LockReleaseAsync(GenerateNewLockKey(resource), lockId);
        }

        public override async Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires)
        {
            if (await _redisClient.StringGetAsync<string>(GenerateNewLockKey(resource)) == lockId)
                await _redisClient.ExpireAsync(GenerateNewLockKey(resource), DateTime.Now.Add(timeUntilExpires));
        }

        public string Name => "DistributedLock.Redis";
    }
}