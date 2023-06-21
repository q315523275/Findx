using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Locks;

/// <summary>
///     锁基类
/// </summary>
public abstract class LockBase : ILock
{
    /// <summary>
    ///     申请锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="timeUntilExpires"></param>
    /// <param name="isWait"></param>
    /// <param name="renew"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<RLock> AcquireAsync(string resource, TimeSpan? timeUntilExpires = null, bool isWait = false, bool renew = false, CancellationToken cancellationToken = default)
    {
        timeUntilExpires ??= TimeSpan.FromSeconds(30);

        var startTime = DateTime.Now;
        var gotLock = false;
        var lockId = GenerateNewLockId();
        do
        {
            try
            {
                gotLock = await TryLockAsync(resource, lockId, timeUntilExpires.Value);
            }
            catch
            {
                // ignored
            }

            // 拿到锁
            // 线程取消
            // 不需要等待争抢锁
            if (gotLock || cancellationToken.IsCancellationRequested || !isWait)
                break;

            // 循环相关计算
            var delayAmount = timeUntilExpires.Value;

            // delay a minimum of 50ms
            if (delayAmount < TimeSpan.FromMilliseconds(50))
                delayAmount = TimeSpan.FromMilliseconds(50);

            // delay a maximum of 3 seconds
            if (delayAmount > TimeSpan.FromSeconds(3))
                delayAmount = TimeSpan.FromSeconds(3);

            // 到达最大等待时间
            if (startTime.Add(timeUntilExpires.Value) >= DateTime.Now.Add(-delayAmount))
                break;

            await Task.Delay(delayAmount, cancellationToken);

            Thread.Yield();
        } while (!cancellationToken.IsCancellationRequested);

        return !gotLock
            ? null
            : new RLock(resource, lockId, this, timeUntilExpires.Value, renew,
                timeUntilExpires.Value.TotalMilliseconds.To<int>() / 3);
    }

    /// <summary>
    ///     释放锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <returns></returns>
    public abstract Task ReleaseAsync(string resource, string lockId);

    /// <summary>
    ///     锁续期
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="timeUntilExpires"></param>
    /// <returns></returns>
    public abstract Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires);

    /// <summary>
    ///     创建锁标识
    /// </summary>
    /// <returns></returns>
    protected string GenerateNewLockId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     创建锁键值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected string GenerateNewLockKey(string key)
    {
        return $"lock:{key}";
    }

    /// <summary>
    ///     尝试获取锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="timeUntilExpires"></param>
    /// <returns></returns>
    protected abstract Task<bool> TryLockAsync(string resource, string lockId, TimeSpan timeUntilExpires);
}