using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     调度选举器
/// </summary>
public interface IBackgroundScheduleElector
{
    /// <summary>
    ///     设置为调度者
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> PutScheduleLeaderAsync(CancellationToken cancellationToken = default);
}