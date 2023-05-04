namespace Findx.Jobs;

/// <summary>
///     作业执行上下文
/// </summary>
public class JobExecutionContext : IJobExecutionContext
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="jobId"></param>
    /// <param name="executionId"></param>
    /// <param name="fullName"></param>
    public JobExecutionContext(IServiceProvider serviceProvider, long jobId, long executionId, string fullName)
    {
        ServiceProvider = serviceProvider;
        JobId = jobId;
        ExecutionId = executionId;
        FullName = fullName;
    }

    /// <summary>
    ///     服务提供器
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     任务编号
    /// </summary>
    public long JobId { get; }

    /// <summary>
    ///     执行记录编号
    /// </summary>
    public long ExecutionId { get; }

    /// <summary>
    ///     任务全名
    /// </summary>
    public string FullName { get; }

    /// <summary>
    ///     任务名
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     任务参数
    /// </summary>
    public IDictionary<string, string> Parameter { get; set; } = new Dictionary<string, string>();

    /// <summary>
    ///     分片索引
    /// </summary>
    public int ShardIndex { get; set; }

    /// <summary>
    ///     分片总数
    /// </summary>
    public int ShardTotal { get; set; }
}