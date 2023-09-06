using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Utilities;

/// <summary>
///     重试辅助操作
/// </summary>
public static class RetryUtility
{
    /// <summary>
    ///     执行同步方法
    /// </summary>
    /// <param name="action"></param>
    /// <param name="maxRetryTimes"></param>
    /// <param name="onRetry"></param>
    /// <param name="delayFunc"></param>
    /// <returns></returns>
    public static bool TryInvoke(Action action, int maxRetryTimes = 3, Action<int, TimeSpan, Exception> onRetry = null,
        Func<int, TimeSpan> delayFunc = null)
    {
        Check.NotNull(action, nameof(action));

        var time = 0;
        do
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                time++;
                var delay = delayFunc?.Invoke(time);
                onRetry?.Invoke(time, delay.GetValueOrDefault(), ex);
                if (delay.HasValue) Thread.Sleep(delay.Value);
            }
        } while (time <= maxRetryTimes);

        return false;
    }

    /// <summary>
    ///     执行异步方法
    /// </summary>
    /// <param name="func"></param>
    /// <param name="maxRetryTimes"></param>
    /// <param name="onRetry"></param>
    /// <param name="delayFunc"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> TryInvokeAsync(Func<Task> func, int maxRetryTimes = 3,
        Func<int, TimeSpan, Exception, Task> onRetry = null, Func<int, TimeSpan> delayFunc = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(func, nameof(func));

        var time = 0;
        do
        {
            try
            {
                await func();
                return true;
            }
            catch (Exception ex)
            {
                time++;
                var delay = delayFunc?.Invoke(time);
                await onRetry?.Invoke(time, delay.GetValueOrDefault(), ex)!;
                if (delay.HasValue) await Task.Delay(delay.Value, cancellationToken);
            }
        } while (time <= maxRetryTimes && !cancellationToken.IsCancellationRequested);

        return false;
    }
}