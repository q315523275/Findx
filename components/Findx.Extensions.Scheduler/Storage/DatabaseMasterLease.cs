using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     基于数据表的 Master 租约（单行 + 条件 UPDATE，MySQL/SQLServer/PG 语义一致）。
///     <para>语句语义：<c>UPDATE ... SET owner=@me, lease_until=@until WHERE id=1 AND (owner=@me OR lease_until&lt;=@now)</c>，
///     影响行数=1 表示当前持有。获取与续期是同一条语句，无需区分。</para>
/// </summary>
public class DatabaseMasterLease : IMasterLease
{
    private const long MasterRowId = 1;

    private readonly IServiceProvider _serviceProvider;
    private readonly IClusterClock _clock;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ILogger<DatabaseMasterLease> _logger;
    private readonly object _seedLock = new();
    private bool _seeded;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="clock"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public DatabaseMasterLease(IServiceProvider serviceProvider, IClusterClock clock, IOptions<SchedulerOptions> options, ILogger<DatabaseMasterLease> logger)
    {
        _serviceProvider = serviceProvider;
        _clock = clock;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    ///     尝试获取或续期
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="ttl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> TryAcquireAsync(string owner, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        await EnsureRowAsync(cancellationToken);

        var now = _clock.Now;
        var until = now.Add(ttl);
        var affected = await DbAccess.UseAsync<SchedulerMaster, int>(_serviceProvider,
            repo => repo.UpdateColumnsAsync(
                x => new SchedulerMaster { Owner = owner, LeaseUntil = until, UpdatedAt = now },
                x => x.Id == MasterRowId && (x.Owner == owner || x.LeaseUntil <= now),
                cancellationToken), cancellationToken);

        return affected > 0;
    }

    /// <summary>
    ///     释放租约（仅当持有者是自己）
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ReleaseAsync(string owner, CancellationToken cancellationToken = default)
    {
        var now = _clock.Now;
        var affected = await DbAccess.UseAsync<SchedulerMaster, int>(_serviceProvider,
            repo => repo.UpdateColumnsAsync(
                x => new SchedulerMaster { LeaseUntil = now.AddSeconds(-1), UpdatedAt = now },
                x => x.Id == MasterRowId && x.Owner == owner,
                cancellationToken), cancellationToken);
        return affected > 0;
    }

    /// <summary>
    ///     当前有效持有者
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetCurrentOwnerAsync(CancellationToken cancellationToken = default)
    {
        var row = await DbAccess.UseAsync<SchedulerMaster, SchedulerMaster>(_serviceProvider, repo => repo.GetAsync(MasterRowId, cancellationToken), cancellationToken);
        if (row == null) return null;
        return row.LeaseUntil > _clock.Now ? row.Owner : null;
    }

    /// <summary>
    ///     保证单行存在（并发建行时忽略唯一键冲突）
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task EnsureRowAsync(CancellationToken cancellationToken)
    {
        if (_seeded) return;

        lock (_seedLock)
        {
            if (_seeded) return;
        }

        var seeded = false;
        var exists = await DbAccess.UseAsync<SchedulerMaster, bool>(_serviceProvider, repo => repo.ExistAsync(x => x.Id == MasterRowId, cancellationToken), cancellationToken);

        if (exists)
        {
            seeded = true;
        }
        else
        {
            try
            {
                await DbAccess.UseAsync<SchedulerMaster, int>(_serviceProvider,
                    repo => repo.InsertAsync(new SchedulerMaster
                    {
                        Id = MasterRowId,
                        LockKey = _options.Value.MasterLockKey,
                        Owner = string.Empty,
                        LeaseUntil = DateTime.MinValue,
                        UpdatedAt = _clock.Now
                    }, cancellationToken), cancellationToken);
                seeded = true;
            }
            catch (Exception ex)
            {
                //  多节点同时首次启动：唯一键冲突说明别的节点已经建好，忽略
                _logger.LogDebug(ex, "Master 租约表初始化行已由其他节点创建");
            }
        }

        //  只有确认行存在才标记完成：建行遇到瞬时故障时下次调用会重试，避免本节点永远无法当选
        if (seeded)
        {
            lock (_seedLock)
            {
                _seeded = true;
            }
        }
    }
}
