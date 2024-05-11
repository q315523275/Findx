using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     调度任务服务端触发器
/// </summary>
public interface IBackgroundJobTriggerServer
{
    /// <summary>
    ///     触发任务执行
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TriggerAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default);
}