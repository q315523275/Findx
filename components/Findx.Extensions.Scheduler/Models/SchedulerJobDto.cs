namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     人工终止请求（调度端 -> 执行端）
/// </summary>
public class KillRequest
{
    /// <summary>
    ///     执行链路编号
    /// </summary>
    public string TraceId { get; set; }
}

/// <summary>
///     任务定义维护入参（管理面新增/修改）
/// </summary>
public class SchedulerJobDto
{
    /// <summary>
    ///     记录编号（新增时留空）
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    ///     任务逻辑名（Worker 能力匹配键之一）
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     作业实现类全名
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     Cron 表达式（5 段或 6 段含秒）
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    ///     执行参数（JSON 字符串，原样透传）
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Remark { get; set; }
}
