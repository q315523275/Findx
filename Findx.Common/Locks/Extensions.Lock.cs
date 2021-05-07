using System;
using System.Threading.Tasks;

namespace Findx.Locks
{
    /// <summary>
    /// 扩展 - 进程锁
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 使用锁执行一个方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public static void ExecuteWithLock(this ILock _lock, string key, string value, TimeSpan span, Action executeAction)
        {
            if (executeAction == null)
                return;

            if (_lock.LockTake(key, value, span))
            {
                try
                {
                    executeAction();
                }
                finally
                {
                    _lock.LockRelease(key, value);
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
        public static T ExecuteWithLock<T>(this ILock _lock, string key, string value, TimeSpan span, Func<T> executeAction, T defaultValue = default)
        {
            if (executeAction == null)
                return defaultValue;

            if (_lock.LockTake(key, value, span))
            {
                try
                {
                    return executeAction();
                }
                finally
                {
                    _lock.LockRelease(key, value);
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
        public static async Task ExecuteWithLockAsync(this ILock _lock, string key, string value, TimeSpan span, Func<Task> executeAction)
        {
            if (executeAction == null)
                return;

            if (await _lock.LockTakeAsync(key, value, span))
            {
                try
                {
                    await executeAction();
                }
                finally
                {
                    await _lock.LockReleaseAsync(key, value);
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
        public static async Task<T> ExecuteWithLockAsync<T>(this ILock _lock, string key, string value, TimeSpan span, Func<Task<T>> executeAction, T defaultValue = default)
        {
            if (executeAction == null)
                return defaultValue;

            if (await _lock.LockTakeAsync(key, value, span))
            {
                try
                {
                    return await executeAction();
                }
                finally
                {
                    await _lock.LockReleaseAsync(key, value);
                }
            }
            return defaultValue;
        }
    }
}
