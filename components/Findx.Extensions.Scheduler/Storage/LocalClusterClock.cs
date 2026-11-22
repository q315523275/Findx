namespace Findx.Extensions.Scheduler.Storage;

using Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     本地时钟（默认实现）。
///     <para>可选校准：若数据库与应用的时区一致，可在启动时用数据库 CURRENT_TIMESTAMP 调一次
///     <see cref="Calibrate"/>，把数据库与应用的时钟偏差收敛掉；时区不一致时不要校准。</para>
/// </summary>
public class LocalClusterClock : IClusterClock
{
    private long _offsetTicks;

    /// <summary>
    ///     当前时刻（本地时间 + 校准偏移）
    /// </summary>
    public DateTime Now => DateTime.Now.AddTicks(Interlocked.Read(ref _offsetTicks));

    /// <summary>
    ///     设置校准偏移（数据库时间 - 本地时间）
    /// </summary>
    /// <param name="offset"></param>
    public void Calibrate(TimeSpan offset)
    {
        Interlocked.Exchange(ref _offsetTicks, offset.Ticks);
    }
}
