using Findx.Locks;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 侧在途执行记录
/// </summary>
public class InFlightExecution
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
    ///     作业实现类全名
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     Cron 表达式（死信后推进下一周期用）
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    ///     执行参数
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     本次触发周期标识
    /// </summary>
    public string Period { get; set; }

    /// <summary>
    ///     当前是第几次尝试（0=首次分发）
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    ///     分发到的 Worker 地址
    /// </summary>
    public string WorkerAddress { get; set; }

    /// <summary>
    ///     分发开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    ///     本次执行持有的执行权锁（结果回报或超时后释放）
    /// </summary>
    public RLock ExecutionLock { get; set; }
}

/// <summary>
///     Master 侧在途执行记录表（TraceId -> 执行上下文，仅存活于 Master 内存）
/// </summary>
public class InFlightExecutionRegistry
{
    private readonly ConcurrentDictionary<string, InFlightExecution> _items = new();

    /// <summary>
    ///     登记在途执行
    /// </summary>
    public void Add(InFlightExecution execution)
    {
        _items[execution.TraceId] = execution;
    }

    /// <summary>
    ///     移除并取出在途执行（收到结果回调时）
    /// </summary>
    public bool TryRemove(string traceId, out InFlightExecution execution)
    {
        return _items.TryRemove(traceId, out execution);
    }

    /// <summary>
    ///     快照全部在途执行（超时兜底扫描用）
    /// </summary>
    public IReadOnlyList<InFlightExecution> GetAll()
    {
        return _items.Values.ToList();
    }
}
