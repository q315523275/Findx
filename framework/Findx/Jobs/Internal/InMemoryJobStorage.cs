using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Internal;

/// <summary>
///     工作内存存储器
/// </summary>
public class InMemoryJobStorage : IJobStorage
{
    private readonly List<JobInfo> _jobs;

    /// <summary>
    ///     Ctor
    /// </summary>
    public InMemoryJobStorage()
    {
        // 字典存储也可以
        _jobs = new List<JobInfo>();
    }

    /// <summary>
    ///     删除工作
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public Task DeleteAsync(long jobId)
    {
        _jobs.RemoveAll(x => x.Id == jobId);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     查询单个工作信息
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public Task<JobInfo> FindAsync(long jobId)
    {
        return Task.FromResult(_jobs.FirstOrDefault(x => x.Id == jobId));
    }

    /// <summary>
    ///     获取全部工作
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<JobInfo>> GetJobsAsync()
    {
        return Task.FromResult(_jobs.AsEnumerable());
    }

    /// <summary>
    ///     获取指定数量的待执行工作
    /// </summary>
    /// <param name="maxResultCount"></param>
    /// <returns></returns>
    public Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount)
    {
        var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
        var tasksThatShouldRun = _jobs.Where(t => t != null && t.ShouldRun(referenceTime))
            .OrderBy(t => t.TryCount)
            .ThenBy(t => t.NextRunTime)
            .Take(maxResultCount);
        return Task.FromResult(tasksThatShouldRun);
    }

    /// <summary>
    ///     插入
    /// </summary>
    /// <param name="detail"></param>
    /// <returns></returns>
    public Task InsertAsync(JobInfo detail)
    {
        _jobs.TryAdd(detail);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     更新
    /// </summary>
    /// <param name="detail"></param>
    /// <returns></returns>
    public Task UpdateAsync(JobInfo detail)
    {
        _jobs.ReplaceOne(x => x.Id == detail.Id, detail);
        return Task.CompletedTask;
    }
}