using Findx.Data;
using Findx.Extensions.Scheduler.Models;
using Findx.Jobs.Common;
using Findx.NewId;
using Findx.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     调度任务持久化存储（FreeSql IRepository，数据源 "jobs"）。
///     <para>IRepository 为 Scoped 生命周期，本类以单例存在，故每个操作自建 Scope 解析仓储。</para>
/// </summary>
public class ScheduledJobStorage
{
    private readonly ILogger<ScheduledJobStorage> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyGenerator<long> _keyGenerator;

    /// <summary>
    ///     任务定义变更通知：增删改、失败退避、死信推进后触发，供 Master 内存窗口标脏（下次调度后台重查）
    /// </summary>
    public event Action JobsChanged;

    /// <summary>
    ///     通知任务定义已变更
    /// </summary>
    private void NotifyChanged() => JobsChanged?.Invoke();

    /// <summary>
    ///     Ctor
    /// </summary>
    public ScheduledJobStorage(IServiceProvider serviceProvider, IKeyGenerator<long> keyGenerator,
        ILogger<ScheduledJobStorage> logger)
    {
        _serviceProvider = serviceProvider;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     在作用域内执行仓储操作
    /// </summary>
    private async Task<T> WithRepositoryAsync<T>(Func<IRepository<ScheduledJob, long>, Task<T>> action, CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ScheduledJob, long>>();
        return await action(repo);
    }

    /// <summary>
    ///     在作用域内执行仓储操作
    /// </summary>
    private async Task WithRepositoryAsync(Func<IRepository<ScheduledJob, long>, Task> action, CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ScheduledJob, long>>();
        await action(repo);
    }

    /// <summary>
    ///     新增任务定义；NextRunTime 为空时按 Cron 立即计算首次触发时间
    /// </summary>
    public async Task<ScheduledJob> AddAsync(ScheduledJob job, CancellationToken cancellationToken = default)
    {
        if (job.NextRunTime == null && !string.IsNullOrWhiteSpace(job.CronExpression))
            job.NextRunTime = CronUtility.GetNextOccurrence(job.CronExpression, DateTimeOffset.Now);

        //  scheduler_jobs 由 UseAutoSyncStructure 建表，id 列没有 AUTO_INCREMENT，仓储插入也不会补主键：
        //  不显式赋值会让所有任务都以 Id=0 落库，第二条任务直接主键冲突。主键统一走框架 IKeyGenerator<long>（默认雪花）。
        if (job.Id == 0) job.Id = _keyGenerator.Create();

        await WithRepositoryAsync(async repo => { await repo.InsertAsync(job, cancellationToken); }, cancellationToken);
        NotifyChanged();
        return job;
    }

    /// <summary>
    ///     更新任务定义
    /// </summary>
    public async Task<int> UpdateAsync(ScheduledJob job, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(repo => repo.UpdateAsync(job, cancellationToken: cancellationToken), cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     按 Id 查询
    /// </summary>
    public async Task<ScheduledJob> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(repo => repo.GetAsync(id, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     按 Id 删除
    /// </summary>
    public async Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(repo => repo.DeleteAsync(id, cancellationToken), cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     查询全部任务定义
    /// </summary>
    public async Task<List<ScheduledJob>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(repo => repo.SelectAsync(x => true, cancellationToken: cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     查询窗口内任务（启用 且（NextRunTime 或 RetryAtTime 落在窗口终点之前））。
    ///     <para>含已过期未触发的任务：Master 拉进内存后补触发一次，再按 Cron 推进。</para>
    /// </summary>
    public async Task<List<ScheduledJob>> GetWindowJobsAsync(DateTime windowEnd, CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(
            repo => repo.SelectAsync(x => x.Status == JobStatus.Normal &&
                                          ((x.NextRunTime != null && x.NextRunTime <= windowEnd) ||
                                           (x.RetryAtTime != null && x.RetryAtTime <= windowEnd)), cancellationToken: cancellationToken),
            cancellationToken);
    }

    /// <summary>
    ///     无节点时的周期延后：以 CAS 把周期锚点从 <paramref name="expectedNext"/> 推进到 <paramref name="newNext"/>，
    ///     同时写入退避时刻，且不产生执行流水。
    ///     <para>必须推进锚点：否则本轮周期仍是「已到期未领取」，等重试分发完一次后同一周期会被再次领走，造成重复执行。</para>
    /// </summary>
    /// <param name="id">任务编号</param>
    /// <param name="expectedNext">CAS 期望的当前 NextRunTime</param>
    /// <param name="newNext">推进后的 NextRunTime</param>
    /// <param name="lastRun">本次触发时刻（写入 LastRunTime）</param>
    /// <param name="retryAt">退避时刻</param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数（0 表示锚点已被其他节点推进，本次放弃）</returns>
    public async Task<int> DeferCycleAsync(long id, DateTime expectedNext, DateTime newNext, DateTime lastRun,
        DateTime retryAt, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { NextRunTime = newNext, LastRunTime = lastRun, RetryAtTime = retryAt },
                x => x.Id == id && x.NextRunTime == expectedNext && x.Status == JobStatus.Normal,
                cancellationToken),
            cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     记录一次失败：写入连续失败次数并把重试时刻设为退避后的时间（不动 NextRunTime，周期锚点不偏移）
    /// </summary>
    public async Task<int> ApplyFailureAsync(long id, int tryCount, DateTime? retryAtTime, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { TryCount = tryCount, RetryAtTime = retryAtTime },
                x => x.Id == id,
                cancellationToken),
            cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     记录一次成功：连续失败次数归零并清空重试标记（NextRunTime 已在抢占时推进）
    /// </summary>
    public async Task<int> ResetSuccessAsync(long id, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(x => new ScheduledJob { TryCount = 0, RetryAtTime = null }, x => x.Id == id, cancellationToken),
            cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     写入死信表（独立作用域，仓储泛型为 JobDeadLetter）
    /// </summary>
    public async Task WriteDeadLetterAsync(JobDeadLetter letter, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<JobDeadLetter, long>>();
        if (letter.Id == 0) letter.Id = _keyGenerator.Create();
        await repo.InsertAsync(letter, cancellationToken);
    }

    /// <summary>
    ///     死信隔离：连续失败达到上限后把任务置为 Dead 并清空时间锚点，调度扫描按 Status=Normal 过滤后自然不再取到，
    ///     只能通过 <see cref="ReleaseAsync"/> 人工放行。
    /// </summary>
    /// <param name="id">任务编号</param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    public async Task<int> IsolateAsync(long id, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { Status = JobStatus.Dead, NextRunTime = null, RetryAtTime = null },
                x => x.Id == id,
                cancellationToken),
            cancellationToken);
        NotifyChanged();
        return affected;
    }

    /// <summary>
    ///     人工放行：把 Dead 任务恢复为 Normal，并按 Cron 计算新的下次触发时刻
    /// </summary>
    /// <param name="id">任务编号</param>
    /// <param name="nextRunTime">新的 NextRunTime（调用方按 Cron 计算）</param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数（0 表示任务不存在或不是 Dead 状态）</returns>
    public async Task<int> ReleaseAsync(long id, DateTime nextRunTime, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { Status = JobStatus.Normal, NextRunTime = nextRunTime, RetryAtTime = null, TryCount = 0 },
                x => x.Id == id && x.Status == JobStatus.Dead,
                cancellationToken),
            cancellationToken);
        NotifyChanged();
        return affected;
    }
}
