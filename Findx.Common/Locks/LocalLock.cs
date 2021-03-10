using Findx.Extensions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Locks
{
    public class LocalLock : ILock
    {
        private static readonly ConcurrentDictionary<string, object> _LockCache = new ConcurrentDictionary<string, object>();
        private static readonly ConcurrentDictionary<string, string> _LockUserCache = new ConcurrentDictionary<string, string>();
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

            var obj = _LockCache.GetOrAdd(key, () => { return new object(); });
            if (Monitor.TryEnter(obj, span))
            {
                _LockUserCache[key] = value;
                return true;
            }
            return false;
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
            return Task.FromResult(LockTake(key, value, span));
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

            _LockCache.TryGetValue(key, out object obj);
            if (obj != null)
            {
                if (_LockUserCache[key] == value)
                {
                    Monitor.Exit(obj);
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步释放一个锁
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <returns>成功返回true</returns>
        public Task<bool> LockReleaseAsync(string key, string value)
        {
            return Task.FromResult(LockRelease(key, value));
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
