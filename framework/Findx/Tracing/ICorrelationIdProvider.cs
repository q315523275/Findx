namespace Findx.Tracing;

/// <summary>
///     跟踪标识提供程序
/// </summary>
public interface ICorrelationIdProvider
{
    /// <summary>
    ///     获取跟踪标识
    /// </summary>
    string Get();

    /// <summary>
    ///     变更跟踪标识
    /// </summary>
    /// <param name="correlationId"></param>
    IDisposable Change(string correlationId);
}