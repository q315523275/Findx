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
    // 每实例独立结果对象，避免与共享的 JobResult.Success 单例相互污染
    public JobResult JobResult { set; get; } = new() { IsSuccess = true };

    /// <summary>
    ///     执行日志记录器：由执行宿主注入（分布式调度会把内容采集进该次执行记录）。
    ///     <para>内存版调度或未注入时为 null，调用请使用 <see cref="JobLogRecorderExtensions"/> 的扩展方法（空实现安全）。</para>
    /// </summary>
    public IJobLogRecorder LogRecorder { get; set; }
}