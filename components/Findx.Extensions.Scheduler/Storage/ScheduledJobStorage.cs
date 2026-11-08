using Findx.Data;
using Findx.Extensions.Scheduler.Models;
using Findx.Jobs.Common;
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

    /// <summary>
    ///     Ctor
    /// </summary>
    public ScheduledJobStorage(IServiceProvider serviceProvider, ILogger<ScheduledJobStorage> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     在作用域内执行仓储操作
    /// </summary>
    private async Task<T> WithRepositoryAsync<T>(Func<IRepository<ScheduledJob, long>, Task<T>> action, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ScheduledJob, long>>();
        return await action(repo);
    }

    /// <summary>
    ///     在作用域内执行仓储操作
    /// </summary>
    private async Task WithRepositoryAsync(Func<IRepository<ScheduledJob, long>, Task> action, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
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

        await WithRepositoryAsync(async repo => { await repo.InsertAsync(job, cancellationToken); }, cancellationToken);
        return job;
    }

    /// <summary>
    ///     更新任务定义
    /// </summary>
    public async Task<int> UpdateAsync(ScheduledJob job, CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(repo => repo.UpdateAsync(job, cancellationToken: cancellationToken), cancellationToken);
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
        return await WithRepositoryAsync(repo => repo.DeleteAsync(id, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     查询全部任务定义
    /// </summary>
    public async Task<List<ScheduledJob>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(repo => repo.SelectAsync(x => true, cancellationToken: cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     查询到期任务（启用 且（NextRunTime 到期 或 RetryAtTime 重试到期））
    /// </summary>
    public async Task<List<ScheduledJob>> GetDueJobsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await WithRepositoryAsync(
            repo => repo.SelectAsync(x => x.Status == JobStatus.Normal &&
                                          ((x.NextRunTime != null && x.NextRunTime <= now) ||
                                           (x.RetryAtTime != null && x.RetryAtTime <= now)), cancellationToken: cancellationToken),
            cancellationToken);
    }

    /// <summary>
    ///     周期 CAS 抢占：仅当 NextRunTime 仍等于期望值时才推进，返回是否抢占成功（0 行影响=已被其他 Master 抢占）
    /// </summary>
    public async Task<bool> ClaimDueAsync(long id, DateTime expectedNextRunTime, DateTime newNextRunTime, DateTime lastRunTime, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { NextRunTime = newNextRunTime, LastRunTime = lastRunTime },
                x => x.Id == id && x.NextRunTime == expectedNextRunTime,
                cancellationToken),
            cancellationToken);
        return affected > 0;
    }

    /// <summary>
    ///     重试 CAS 抢占：仅当 RetryAtTime 仍等于期望值时才清空（抢到者负责本次重试分发），返回是否抢占成功。
    ///     只清重试标记，不动 NextRunTime——周期锚点保持原推进结果
    /// </summary>
    public async Task<bool> ClaimRetryAsync(long id, DateTime expectedRetryAtTime, CancellationToken cancellationToken = default)
    {
        var affected = await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { RetryAtTime = null },
                x => x.Id == id && x.RetryAtTime == expectedRetryAtTime,
                cancellationToken),
            cancellationToken);
        return affected > 0;
    }

    /// <summary>
    ///     记录一次失败：写入连续失败次数并把重试时刻设为退避后的时间（不动 NextRunTime，周期锚点不偏移）
    /// </summary>
    public async Task ApplyFailureAsync(long id, int tryCount, DateTime? retryAtTime, CancellationToken cancellationToken = default)
    {
        await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { TryCount = tryCount, RetryAtTime = retryAtTime },
                x => x.Id == id,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>
    ///     记录一次成功：连续失败次数归零并清空重试标记（NextRunTime 已在抢占时推进）
    /// </summary>
    public async Task ResetSuccessAsync(long id, CancellationToken cancellationToken = default)
    {
        await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(x => new ScheduledJob { TryCount = 0, RetryAtTime = null }, x => x.Id == id, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    ///     写入死信表（独立作用域，仓储泛型为 JobDeadLetter）
    /// </summary>
    public async Task WriteDeadLetterAsync(JobDeadLetter letter, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<JobDeadLetter, long>>();
        await repo.InsertAsync(letter, cancellationToken);
    }

    /// <summary>
    ///     死信收尾：TryCount 归零（恢复下一周期重试预算），NextRunTime 推进到 Cron 下一触发时刻（避免停留在过去的退避时刻导致下轮扫描提前触发）。
    ///     Cron 缺失/非法时 NextRunTime 置 null，任务停摆待人工修复。
    /// </summary>
    public async Task ApplyDeadLetterAsync(long id, string cronExpression, DateTime now, CancellationToken cancellationToken = default)
    {
        DateTime? nextRunTime = null;
        if (!string.IsNullOrWhiteSpace(cronExpression))
        {
            try
            {
                var next = CronUtility.GetNextOccurrence(cronExpression, new DateTimeOffset(now));
                nextRunTime = next == default ? null : next;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "任务 {JobId} Cron 表达式 {Cron} 无法计算下次触发时间，死信后 NextRunTime 置空", id, cronExpression);
            }
        }

        await WithRepositoryAsync(
            repo => repo.UpdateColumnsAsync(
                x => new ScheduledJob { TryCount = 0, RetryAtTime = null, NextRunTime = nextRunTime },
                x => x.Id == id,
                cancellationToken),
            cancellationToken);
    }
}
