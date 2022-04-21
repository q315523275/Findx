using Findx.Caching;
using Findx.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Locks
{
    public class LocalCacheLock : LockBase
    {

        private readonly ICache _cache;

        public LocalCacheLock(ICacheProvider provider)
        {
            _cache = provider.Get(CacheType.DefaultMemory);
        }

        public override LockType LockType => LockType.Local;

        public override async Task ReleaseAsync(string resource, string lockId)
        {
            var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource));
            if (v.IsNullOrWhiteSpace() || v != lockId)
                return;

            await _cache.RemoveAsync(GenerateNewLockKey(resource));
        }

        public override async Task RenewAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource));
            if (!v.IsNullOrWhiteSpace() && v == lockId)
            {
                await _cache.AddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires);
            }
        }

        public override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            return await _cache.TryAddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires);
        }
    }
}
