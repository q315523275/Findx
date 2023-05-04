using System.Threading.Tasks;

namespace Findx.Locks;

/// <summary>
///     扩展 - 进程锁
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     是否锁定
    /// </summary>
    /// <param name="lock"></param>
    /// <returns></returns>
    public static bool IsLocked(this RLock @lock)
    {
        return @lock != null;
    }

    /// <summary>
    ///     使用锁执行一个异步方法
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="key">锁的键</param>
    /// <param name="span">耗时时间</param>
    /// <param name="executeAction">要执行的方法</param>
    public static async Task ExecuteWithLockAsync(this ILock @lock, string key, TimeSpan span, Func<Task> executeAction)
    {
        if (executeAction == null)
            return;

        var rlock = await @lock.AcquireAsync(key, span);
        if (rlock.IsLocked())
            try
            {
                executeAction();
            }
            finally
            {
                await rlock.ReleaseAsync();
            }
    }

    /// <summary>
    ///     使用锁执行一个异步方法
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="lock"></param>
    /// <param name="key">锁的键</param>
    /// <param name="span">锁时长</param>
    /// <param name="executeAction">要执行的方法</param>
    /// <param name="defaultValue">默认返回</param>
    /// <returns></returns>
    public static async Task<T> ExecuteWithLockAsync<T>(this ILock @lock, string key, TimeSpan span,
        Func<Task<T>> executeAction, T defaultValue = default)
    {
        if (executeAction == null)
            return defaultValue;

        var rlock = await @lock.AcquireAsync(key, span);
        if (!rlock.IsLocked()) return defaultValue;
        try
        {
            return await executeAction();
        }
        finally
        {
            await rlock.ReleaseAsync();
        }
    }
}