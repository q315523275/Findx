namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     Master -> Worker 任务分发请求体（POST {workerAddress}/api/scheduler/jobs/execute）
/// </summary>
public class DispatchRequest
{
    /// <summary>
    ///     调度任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     任务逻辑名
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     作业实现类全名
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     执行参数（JSON）
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     执行链路编号（Worker 回调原样带回）
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    ///     本次触发周期标识（到期时刻，执行权锁分组键）
    /// </summary>
    public string Period { get; set; }

    /// <summary>
    ///     Master 回调地址（Worker 执行完 POST 结果到这里）
    /// </summary>
    public string MasterAddress { get; set; }
}
