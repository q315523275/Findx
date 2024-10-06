using System.Threading.Tasks;
using Findx.Threading;

namespace Findx.Jobs.Server;

/// <summary>
///     定义一个调度工作者
/// </summary>
public interface IBackgroundScheduleServer : IRunnable
{
    /// <summary>
    ///     任务调度
    /// </summary>
    /// <param name="jobInfo"></param>
    /// <param name="nowTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DispatchAsync(JobInfo jobInfo, DateTimeOffset nowTime, CancellationToken cancellationToken = default);
}