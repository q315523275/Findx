using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Storage;
using Findx.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 侧「到期任务内存窗口」：把未来一个窗口内（含已过期未触发）的任务一次性拉到内存，
///     调度循环只做内存比对与推进，数据库查询退化为「窗口耗尽/定义变更时的一次后台刷新」。
///     <para>推进采用乐观策略：内存中先推进 NextRunTime 再入队，真实写库由分发线程 CAS 完成；
///     CAS 失败（周期已被其他实例处理）时把该任务从窗口剔除并触发重查，保证不重复分发。</para>
///     <para>一致性：任务增删改会通过 ScheduledJobStorage.JobsChanged 通知标脏，最长一个刷新间隔内生效。</para>
/// </summary>
public class SchedulerJobBuffer
{
    /// <summary>
    ///     两次刷新尝试的最小间隔（秒），避免定义频繁变更时反复打库
    /// </summary>
    private const int MinRefreshIntervalSeconds = 1;

    private readonly ScheduledJobStorage _storage;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ILogger<SchedulerJobBuffer> _logger;

    private readonly Dictionary<long, ScheduledJob> _jobs = new();
    private readonly object _sync = new object();

    private DateTime _lastLoadedAt;
    private DateTime _lastAttemptAt;
    private volatile bool _dirty = true;
    private int _refreshing;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SchedulerJobBuffer(ScheduledJobStorage storage, IOptions<SchedulerOptions> options, ILogger<SchedulerJobBuffer> logger)
    {
        _storage = storage;
        _options = options;
        _logger = logger;

        //  任务定义变更（增删改、失败退避、死信推进）后标脏，下轮调度触发后台重查
        _storage.JobsChanged += MarkDirty;
    }

    /// <summary>
    ///     标记窗口需要重新加载（下轮调度时后台刷新）
    /// </summary>
    public void MarkDirty() => _dirty = true;

    /// <summary>
    ///     剔除一个任务（CAS 抢占失败时调用：本实例的推进未被落库，说明周期已被其他实例处理）
    /// </summary>
    public void Remove(long jobId)
    {
        lock (_sync)
        {
            _jobs.Remove(jobId);
        }
        _dirty = true;
    }

    /// <summary>
    ///     取当前已到期的分发条目（纯内存操作；首次调用会同步加载一次窗口）
    /// </summary>
    public async Task<List<DispatchItem>> ScanDueAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        if (_lastLoadedAt == default)
        {
            //  首次（Master 当选/启动）：同步加载一次，保证第一轮就有任务可调度
            await LoadAsync(now, cancellationToken);
        }
        else
        {
            EnsureRefresh(now);
        }

        List<ScheduledJob> snapshot;
        lock (_sync)
        {
            snapshot = new List<ScheduledJob>(_jobs.Count);
            foreach (var job in _jobs.Values) snapshot.Add(job);
        }

        //  按最近动作时间排序，保证同一轮按到期先后入队（队列消费侧不保证顺序）
        snapshot.Sort((a, b) => NullableCompare(ActionTime(a), ActionTime(b)));

        var items = new List<DispatchItem>();
        foreach (var job in snapshot)
        {
            if (cancellationToken.IsCancellationRequested) break;

            //  分支一：重试到期（优先处理，避免重试长期积压）
            if (job.RetryAtTime != null && job.RetryAtTime <= now)
            {
                var expectedRetry = job.RetryAtTime.Value;
                job.RetryAtTime = null; // 内存先清，真实清库由 CAS 完成
                items.Add(new DispatchItem(job, new[] { "R:" + expectedRetry.ToString("O") }, job.TryCount,
                    DispatchKind.Retry, expectedRetry));
                continue;
            }

            //  分支二：周期到期
            if (job.NextRunTime == null || job.NextRunTime > now) continue;

            if (string.IsNullOrWhiteSpace(job.CronExpression))
            {
                _logger.LogWarning("任务 [{JobName}] Cron 表达式为空，跳过", job.JobName);
                continue;
            }

            var opts = _options.Value;
            var windowSeconds = EffectiveWindowSeconds();
            var expectedNext = job.NextRunTime!.Value;

            //  迟到超过一个预取窗口：按 MisfireStrategy 决定是否补触发
            if ((now - expectedNext).TotalSeconds > windowSeconds && opts.MisfireStrategy == SchedulerMisfireStrategy.DoNothing)
            {
                var skipNext = TryGetNextOccurrence(job, now);
                if (skipNext == default) continue;

                job.NextRunTime = skipNext;
                job.LastRunTime = now;
                _logger.LogWarning("任务 [{JobName}] 迟到 {Late} 秒超过窗口 {Window} 秒，按 DoNothing 策略只推进不触发",
                    job.JobName, (int)(now - expectedNext).TotalSeconds, windowSeconds);
                continue;
            }

            DateTime newNext;
            try
            {
                newNext = CronUtility.GetNextOccurrence(job.CronExpression, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务 [{JobName}] Cron 表达式 {Cron} 解析失败", job.JobName, job.CronExpression);
                continue;
            }

            if (newNext == default)
            {
                _logger.LogWarning("任务 [{JobName}] Cron 表达式 {Cron} 无法计算下次触发时间，跳过", job.JobName, job.CronExpression);
                continue;
            }

            //  待投递的触发点：默认只补当前这一次
            var periods = new List<string> { expectedNext.ToString("O") };

            //  补齐 now 之前错过的多个触发点（对齐内存版 Server 的 while 入环语义，受 CatchUpMaxCount 限制）
            if (opts.CatchUpWithinWindow)
            {
                var maxCount = Math.Max(1, opts.CatchUpMaxCount);
                var cursor = expectedNext;
                while (periods.Count < maxCount)
                {
                    var nextAfterCursor = TryGetNextOccurrence(job, cursor);
                    if (nextAfterCursor == default || nextAfterCursor > now) break;
                    periods.Add(nextAfterCursor.ToString("O"));
                    cursor = nextAfterCursor;
                }
            }

            //  内存先推进到 now 之后的下一次触发，真实写库由分发线程 CAS 完成
            job.NextRunTime = newNext;
            job.LastRunTime = now;

            var item = new DispatchItem(job, periods, job.TryCount, DispatchKind.Cycle, expectedNext);
            item.NewNextRunTime = newNext;
            item.LastRunTime = now;
            items.Add(item);
        }

        return items;
    }

    /// <summary>
    ///     是否需要在后台刷新窗口
    /// </summary>
    private void EnsureRefresh(DateTime now)
    {
        if (!NeedRefresh(now)) return;
        if (Interlocked.CompareExchange(ref _refreshing, 1, 0) != 0) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await LoadAsync(now, CancellationToken.None);
            }
            catch (Exception ex)
            {
                //  刷新失败保留旧窗口，下轮继续尝试（不阻塞调度）
                _logger.LogWarning(ex, "预取窗口刷新失败，沿用当前内存快照");
            }
            finally
            {
                Interlocked.Exchange(ref _refreshing, 0);
            }
        });
    }

    /// <summary>
    ///     是否需要刷新：标脏 / 超过刷新间隔 / 窗口耗尽（按窗口的一半节奏提前扩窗）
    /// </summary>
    private bool NeedRefresh(DateTime now)
    {
        var opts = _options.Value;
        var windowSeconds = Math.Max(1, opts.LookaheadWindowSeconds);
        var refreshSeconds = Math.Max(1, opts.LookaheadRefreshSeconds);

        //  统一最小间隔节流：避免定义频繁变更或窗口耗尽时反复打库
        if (_lastAttemptAt != default && (now - _lastAttemptAt).TotalSeconds < MinRefreshIntervalSeconds) return false;

        if (_dirty) return true;
        if (_lastLoadedAt != default && (now - _lastLoadedAt).TotalSeconds >= refreshSeconds) return true;

        var min = MinActionTime();
        //  空窗口（无任务或任务都已推进到窗口外）：交给上面的定期刷新，不额外触发
        if (min == null) return false;

        //  窗口耗尽：缓存里最早的到期时刻已在窗口之外，需要往后扩窗
        if (min.Value <= now.AddSeconds(windowSeconds)) return false;
        return _lastLoadedAt == default || (now - _lastLoadedAt).TotalSeconds >= Math.Max(MinRefreshIntervalSeconds, windowSeconds / 2);
    }

    /// <summary>
    ///     加载窗口内任务（一次性拉取，整体替换内存快照）
    /// </summary>
    private async Task LoadAsync(DateTime now, CancellationToken cancellationToken)
    {
        _lastAttemptAt = now;
        var windowEnd = now.AddSeconds(EffectiveWindowSeconds());
        var list = await _storage.GetWindowJobsAsync(windowEnd, cancellationToken);

        lock (_sync)
        {
            _jobs.Clear();
            foreach (var job in list) _jobs[job.Id] = job;
        }

        _lastLoadedAt = now;
        _dirty = false;
        _logger.LogDebug("调度预取窗口已加载，窗口内任务 {Count} 个", list.Count);
    }

    /// <summary>
    ///     有效预取窗口（秒）：窗口必须明显大于刷新间隔，否则「刷新时还没进窗、刷新后已过期」会漏掉任务
    /// </summary>
    private int EffectiveWindowSeconds()
    {
        var opts = _options.Value;
        return Math.Max(Math.Max(1, opts.LookaheadWindowSeconds), Math.Max(1, opts.LookaheadRefreshSeconds) * 2);
    }

    /// <summary>
    ///     安全计算下一次触发时刻（Cron 非法时返回 default）
    /// </summary>
    private DateTime TryGetNextOccurrence(ScheduledJob job, DateTime from)
    {
        try
        {
            return CronUtility.GetNextOccurrence(job.CronExpression, from);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务 [{JobName}] Cron 表达式 {Cron} 解析失败", job.JobName, job.CronExpression);
            return default;
        }
    }

    /// <summary>
    ///     缓存中最小的下次动作时间（RetryAtTime 与 NextRunTime 取小）
    /// </summary>
    private DateTime? MinActionTime()
    {
        lock (_sync)
        {
            DateTime? min = null;
            foreach (var job in _jobs.Values)
            {
                var actionTime = ActionTime(job);
                if (actionTime == null) continue;
                if (min == null || actionTime.Value < min.Value) min = actionTime;
            }
            return min;
        }
    }

    /// <summary>
    ///     任务的最近动作时间（重试与周期取小）
    /// </summary>
    private static DateTime? ActionTime(ScheduledJob job)
    {
        var retry = job.RetryAtTime;
        var next = job.NextRunTime;
        if (retry == null) return next;
        if (next == null) return retry;
        return retry.Value <= next.Value ? retry : next;
    }

    /// <summary>
    ///     可空时间比较（null 排最后）
    /// </summary>
    private static int NullableCompare(DateTime? a, DateTime? b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        return a.Value.CompareTo(b.Value);
    }
}
