using System.Threading.Tasks;

namespace Findx.Jobs.Storage;

/// <summary>
///     定义一个工作存储器
/// </summary>
public interface IJobStorage
{
    /// <summary>
    ///     存储任务信息
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InsertAsync(JobInfo detail, CancellationToken cancellationToken = default);

    /// <summary>
    ///     删除任务信息
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新任务信息
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(JobInfo detail, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     批量更新任务信息
    /// </summary>
    /// <param name="jobs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdatesAsync(IEnumerable<JobInfo> jobs, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询任务信息
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<JobInfo> FindAsync(long jobId, CancellationToken cancellationToken = default);
    

    /// <summary>
    ///     查询可以执行任务列表
    /// </summary>
    /// <param name="maxCount"></param>
    /// <param name="nextRunTime">下次执行时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<JobInfo>> FetchShouldRunJobsAsync(DateTimeOffset nextRunTime, int maxCount, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询所有任务
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<JobInfo>> GetJobsAsync(CancellationToken cancellationToken = default);
}