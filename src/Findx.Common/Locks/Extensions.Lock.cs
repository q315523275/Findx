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
        /// 是否锁定
        /// </summary>
        /// <param name="_lock"></param>
        /// <returns></returns>
        public static bool IsLocked(this RLock _lock)
        {
            return _lock != null;
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public static async Task ExecuteWithLockAsync(this ILock _lock, string key, TimeSpan span, Func<Task> executeAction)
        {
            if (executeAction == null)
                return;

            var rlock = await _lock.AcquireAsync(key, timeUntilExpires: span);
            if (rlock.IsLocked())
            {
                try
                {
                    executeAction();
                }
                finally
                {
                    await rlock.ReleaseAsync();
                }
            }
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="executeAction">要执行的方法</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns></returns>
        public static async Task<T> ExecuteWithLockAsync<T>(this ILock _lock, string key, TimeSpan span, Func<Task<T>> executeAction, T defaultValue = default)
        {
            if (executeAction == null)
                return defaultValue;

            var rlock = await _lock.AcquireAsync(key, timeUntilExpires: span);
            if (rlock.IsLocked())
            {
                try
                {
                    return await executeAction();
                }
                finally
                {
                    await rlock.ReleaseAsync();
                }
            }

            return defaultValue;
        }
    }
}
