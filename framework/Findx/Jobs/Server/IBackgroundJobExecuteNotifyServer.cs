using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     任务执行通知
/// </summary>
public interface IBackgroundJobExecuteNotifyServer
{
    /// <summary>
    ///     通知任务执行
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task NotifyAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default);
}