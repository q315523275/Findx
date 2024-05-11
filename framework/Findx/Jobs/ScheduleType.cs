namespace Findx.Jobs;

/// <summary>
///     调度方式
/// </summary>
public enum ScheduleType
{
    /// <summary>
    ///     单体模式
    /// </summary>
    Simple,
    
    /// <summary>
    ///     分布式
    /// </summary>
    Distributed
}