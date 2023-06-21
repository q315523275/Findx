using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Locks;

namespace Findx.Jobs.Internal;

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
        // 字典存储也可以
        _jobs = new List<JobInfo>();
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
    ///     查询单个工作信息
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<JobInfo> FindAsync(long jobId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_jobs.FirstOrDefault(x => x.Id == jobId));
    }

    /// <summary>
    ///     获取全部工作
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<JobInfo>> GetJobsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_jobs.AsEnumerable());
    }

    /// <summary>
    ///     获取指定数量的待执行工作
    /// </summary>
    /// <param name="maxResultCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount, CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
        var tasksThatShouldRun = _jobs.Where(t => t != null && t.ShouldRun(referenceTime))
                                      .OrderBy(t => t.TryCount)
                                      .ThenBy(t => t.NextRunTime)
                                      .Take(maxResultCount);
        return tasksThatShouldRun;
    }

    /// <summary>
    ///     插入
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InsertAsync(JobInfo detail, CancellationToken cancellationToken = default)
    {
        _jobs.TryAdd(detail);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     更新
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UpdateAsync(JobInfo detail, CancellationToken cancellationToken = default)
    {
        _jobs.ReplaceOne(x => x.Id == detail.Id, detail);
        return Task.CompletedTask;
    }
}