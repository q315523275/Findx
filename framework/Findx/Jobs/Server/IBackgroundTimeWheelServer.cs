using Findx.Threading;

namespace Findx.Jobs.Server;

/// <summary>
///     时间轮任务处理服务器
/// </summary>
public interface IBackgroundTimeWheelServer : IRunnable
{
    /// <summary>
    ///     加入时间轮
    /// </summary>
    /// <param name="ringSecond"></param>
    /// <param name="jobExecuteInfo"></param>
    void PushTimeRing(int ringSecond, JobExecuteInfo jobExecuteInfo);
}