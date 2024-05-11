using System.Threading.Tasks;

namespace Findx.Jobs.Client;

/// <summary>
///    任务执行调度器
/// </summary>
public interface IJobExecuteDispatcher
{
    /// <summary>
    ///     触发任务执行
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default);
}