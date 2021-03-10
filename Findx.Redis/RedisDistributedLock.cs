using Findx.Locks;
using System;
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
        public Task<bool> LockTakeAsync(string key, string value, TimeSpan span)
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
        public Task<bool> LockReleaseAsync(string key, string value)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            return _redisClient.LockReleaseAsync(key, value);
        }

        /// <summary>
        /// 使用锁执行一个方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public void ExecuteWithLock(string key, string value, TimeSpan span, Action executeAction)
        {
            if (executeAction == null)
                return;

            if (LockTake(key, value, span))
            {
                try
                {
                    executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
        }

        /// <summary>
        /// 使用锁执行一个方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns></returns>
        public T ExecuteWithLock<T>(string key, string value, TimeSpan span, Func<T> executeAction, T defaultValue = default(T))
        {
            if (executeAction == null)
                return defaultValue;

            if (LockTake(key, value, span))
            {
                try
                {
                    return executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public async Task ExecuteWithLockAsync(string key, string value, TimeSpan span, Func<Task> executeAction)
        {
            if (executeAction == null)
                return;

            if (await LockTakeAsync(key, value, span))
            {
                try
                {
                    await executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns></returns>
        public async Task<T> ExecuteWithLockAsync<T>(string key, string value, TimeSpan span, Func<Task<T>> executeAction, T defaultValue = default)
        {
            if (executeAction == null)
                return defaultValue;

            if (await LockTakeAsync(key, value, span))
            {
                try
                {
                    return await executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
            return defaultValue;
        }
    }
}
