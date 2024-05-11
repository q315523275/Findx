namespace Findx.Jobs;

/// <summary>
///     任务超期策略
/// </summary>
public enum JobMisfireStrategy
{
    /// <summary>
    ///     什么都不做
    /// </summary>
    DoNothing,
    
    /// <summary>
    ///     执行一次
    /// </summary>
    FireOnceNow
}