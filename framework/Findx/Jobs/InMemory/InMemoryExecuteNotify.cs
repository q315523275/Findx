using System.Threading.Tasks;
using Findx.Jobs.Client;
using Findx.Jobs.Server;

namespace Findx.Jobs.InMemory;

/// <summary>
///     内存通知
/// </summary>
public class InMemoryExecuteNotify: IBackgroundJobExecuteNotifyServer
{
    private readonly IJobExecuteDispatcher _dispatcher;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="dispatcher"></param>
    public InMemoryExecuteNotify(IJobExecuteDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    ///     通知执行
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    public async Task NotifyAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default)
    {
        await _dispatcher.SendAsync(jobExecuteInfo, cancellationToken);
    }
}