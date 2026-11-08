namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     Worker -> Master 执行结果回报体（POST {masterAddress}/api/scheduler/jobs/result）
/// </summary>
public class JobResultReport
{
    /// <summary>
    ///     执行链路编号
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    ///     调度任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     任务逻辑名
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     是否成功（取消视为失败处理）
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    ///     错误信息
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    ///     结果消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     执行耗时（毫秒）
    /// </summary>
    public long ElapsedMs { get; set; }

    /// <summary>
    ///     执行节点地址
    /// </summary>
    public string WorkerAddress { get; set; }
}
