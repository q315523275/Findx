using System.Threading.Tasks;

namespace Findx.Locks;

/// <summary>
///     锁
/// </summary>
public interface ILock
{
    /// <summary>
    ///     获取锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="timeUntilExpires"></param>
    /// <param name="isWait"></param>
    /// <param name="renew"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RLock> AcquireAsync(string resource, TimeSpan? timeUntilExpires = null, bool isWait = false,
        bool renew = false, CancellationToken cancellationToken = default);

    /// <summary>
    ///     释放锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <returns></returns>
    Task ReleaseAsync(string resource, string lockId);

    /// <summary>
    ///     更新锁时间，自动续期
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="timeUntilExpires"></param>
    /// <returns></returns>
    Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires);
}