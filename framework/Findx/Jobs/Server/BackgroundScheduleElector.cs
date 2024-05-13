using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     调度选举服务
/// </summary>
public class BackgroundScheduleElector: IBackgroundScheduleElector
{
    /// <summary>
    ///     设置为调度者
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> PutScheduleLeaderAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}