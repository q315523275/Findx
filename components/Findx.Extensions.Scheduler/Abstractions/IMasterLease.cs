namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     Master 租约：集群内同一时刻只有一个持有者。
///     <para>注意：租约只用于减少无效扫描，不承载正确性——"同一周期不重复触发"由任务行 CAS 与执行行唯一键保证，
///     因此租约短时抖动（时钟偏差、网络分区）只会造成多一轮扫描，不会造成重复触发。</para>
/// </summary>
public interface IMasterLease
{
    /// <summary>
    ///     尝试获取或续期租约（同一语句：持有者是我，或原租约已过期）
    /// </summary>
    /// <param name="owner">持有者标识</param>
    /// <param name="ttl">租期</param>
    /// <param name="cancellationToken"></param>
    /// <returns>true=当前持有</returns>
    Task<bool> TryAcquireAsync(string owner, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    ///     主动释放（仅当持有者是自己时生效）
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ReleaseAsync(string owner, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当前有效持有者（无有效租约返回 null）
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetCurrentOwnerAsync(CancellationToken cancellationToken = default);
}
