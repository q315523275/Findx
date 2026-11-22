namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     集群时钟：所有租约、心跳、状态迁移的时间基准都从这里取，避免各调用点直接 DateTime.Now
///     <para>默认实现为本地时钟；如需以数据库时间为准，可替换实现而不改调用方</para>
/// </summary>
public interface IClusterClock
{
    /// <summary>
    ///     当前时刻
    /// </summary>
    DateTime Now { get; }
}
