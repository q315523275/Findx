using Findx.Locks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public class RedisDistributedLock : IDistributedLock
    {
        private readonly IRedisClient _redisClient;

        public RedisDistributedLock(IRedisClientProvider redisClientProvider)
        {
            _redisClient = redisClientProvider.CreateClient();
        }

        /// <summary>
        /// 获取一个锁(需要自己释放)
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <returns>成功返回true</returns>
        public bool LockTake(string key, string value, TimeSpan span)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            Check.NotNull(span, nameof(span));
            return _redisClient.LockTake(key, value, span);
        }

        /// <summary>
        /// 异步获取一个锁(需要自己释放)
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <returns>成功返回true</returns>
        public Task<bool> LockTakeAsync(string key, string value, TimeSpan span, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            Check.NotNull(span, nameof(span));
            return _redisClient.LockTakeAsync(key, value, span);
        }

        /// <summary>
        /// 释放一个锁
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <returns>成功返回true</returns>
        public bool LockRelease(string key, string value)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            return _redisClient.LockRelease(key, value);
        }

        /// <summary>
        /// 异步释放一个锁
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <returns>成功返回true</returns>
        public Task<bool> LockReleaseAsync(string key, string value, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            return _redisClient.LockReleaseAsync(key, value);
        }

        /// <summary>
        /// 续期分布式锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public Task<bool> RefreshTimeToLiveAsync(string key, TimeSpan span)
        {
            Check.NotNull(key, nameof(key));

            return _redisClient.ExpireAsync(key, span);
        }
    }
}
