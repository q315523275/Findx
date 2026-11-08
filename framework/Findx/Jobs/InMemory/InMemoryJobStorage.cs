using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Jobs.Storage;
using Findx.Locks;

namespace Findx.Jobs.InMemory;

/// <summary>
///     工作内存存储器
/// </summary>
public class InMemoryJobStorage : IJobStorage
{
    private readonly List<JobInfo> _jobs;
    private readonly AsyncLock _lock;

    /// <summary>
    ///     Ctor
    /// </summary>
    public InMemoryJobStorage()
    {
        _lock = new AsyncLock();
        _jobs = [];
    }

    /// <summary>
    ///     删除工作
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task DeleteAsync(long jobId, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        _jobs.RemoveAll(x => x.Id == jobId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jobs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task UpdatesAsync(IEnumerable<JobInfo> jobs, CancellationToken cancellationToken = default)
    {
        //  内存版依赖对象引用共享：调度循环通常直接 mutate 同一 JobInfo 引用，
        //  这里再按 Id 做一次替换/合并，保证传入的副本或外部写入也能落到存储
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        foreach (var job in jobs)
        {
            _jobs.ReplaceOne(x => x.Id == job.Id, job);
        }
    }

    /// <summary>
    ///     查询单个工作信息
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<JobInfo> FindAsync(long jobId, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        return _jobs.FirstOrDefault(x => x.Id == jobId);
    }

    /// <summary>
    ///     获取全部工作
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<JobInfo>> GetJobsAsync(CancellationToken cancellationToken = default)
    {
        //  返回快照，避免锁外枚举活 List
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        return _jobs.ToList();
    }

    /// <summary>
    ///     获取指定数量的待执行工作
    /// </summary>
    /// <param name="maxCount"></param>
    /// <param name="nextRunTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<JobInfo>> FetchShouldRunJobsAsync(DateTimeOffset nextRunTime, int maxCount, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        //  锁内立即物化，避免延迟 LINQ 被调用方在锁外枚举（并发 Insert/Delete 会破坏 List 枚举）
        var tasksThatShouldRun = _jobs.Where(x => x.IsEnable && x.NextRunTime.HasValue && x.NextRunTime < nextRunTime)
                                      .OrderBy(t => t.TryCount)
                                      .ThenBy(t => t.NextRunTime)
                                      .Take(maxCount)
                                      .ToList();
        return tasksThatShouldRun;
    }

    /// <summary>
    ///     插入
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InsertAsync(JobInfo detail, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        _jobs.TryAdd(detail);
    }

    /// <summary>
    ///     更新
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task UpdateAsync(JobInfo detail, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        _jobs.ReplaceOne(x => x.Id == detail.Id, detail);
    }
}