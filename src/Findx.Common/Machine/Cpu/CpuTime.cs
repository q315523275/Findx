namespace Findx.Machine.Cpu;

/// <summary>
/// </summary>
public struct CpuTime
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="idleTime"></param>
    /// <param name="systemTime"></param>
    public CpuTime(ulong idleTime, ulong systemTime)
    {
        IdleTime = idleTime;
        SystemTime = systemTime;
    }

    /// <summary>
    ///     Cpu 空闲时间
    /// </summary>
    public ulong IdleTime { get; }

    /// <summary>
    ///     Cpu 工作时间
    /// </summary>
    public ulong SystemTime { get; }
}