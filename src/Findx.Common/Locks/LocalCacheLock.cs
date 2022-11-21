using Findx.Caching;
using Findx.Extensions;
using System.Threading.Tasks;
namespace Findx.Locks
{
    /// <summary>
    /// 本地缓存锁
    /// </summary>
    public class LocalCacheLock : LockBase
    {

        private readonly ICache _cache;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="provider">缓存服务提供器</param>
        public LocalCacheLock(ICacheProvider provider)
        {
            _cache = provider.Get(CacheType.DefaultMemory);
        }

        /// <summary>
        /// 锁类型
        /// </summary>
        public override LockType LockType => LockType.Local;

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="lockId"></param>
        public override async Task ReleaseAsync(string resource, string lockId)
        {
            var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource));
            if (v.IsNullOrWhiteSpace() || v != lockId)
                return;

            await _cache.RemoveAsync(GenerateNewLockKey(resource));
        }

        /// <summary>
        /// 续期
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="lockId"></param>
        /// <param name="timeUntilExpires"></param>
        public override async Task RenewAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource));
            if (!v.IsNullOrWhiteSpace() && v == lockId)
            {
                await _cache.AddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires);
            }
        }

        /// <summary>
        /// 尝试拿锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="lockId"></param>
        /// <param name="timeUntilExpires"></param>
        /// <returns></returns>
        public override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null)
        {
            return await _cache.TryAddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires);
        }
    }
}
