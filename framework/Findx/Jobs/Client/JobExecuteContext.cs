using Findx.Jobs.Common;

namespace Findx.Jobs.Client;

/// <summary>
///     作业执行上下文
/// </summary>
public class JobExecuteContext
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="jobExecuteInfo"></param>
    public JobExecuteContext(IServiceProvider serviceProvider, JobExecuteInfo jobExecuteInfo)
    {
        ServiceProvider = serviceProvider;
        JobExecuteInfo = jobExecuteInfo;
    }

    /// <summary>
    ///     服务提供器
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     任务信息
    /// </summary>
    public JobExecuteInfo JobExecuteInfo { get; }

    /// <summary>
    ///     任务信息
    /// </summary>
    public JobResult JobResult { set; get; } = JobResult.Success;
}