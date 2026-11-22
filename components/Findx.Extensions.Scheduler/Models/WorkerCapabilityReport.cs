using Findx.Jobs.Common;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     节点任务清单覆盖报告（管理面诊断用）。
///     <para>每个执行节点承载的 IJob 可能不同，调度端按 <c>HandlerType</c> 路由；
///     本报告回答两个问题：某个任务由哪些节点承载、哪些任务当前没有任何节点承载。</para>
/// </summary>
public class WorkerCapabilityReport
{
    /// <summary>
    ///     存活节点数
    /// </summary>
    public int AliveWorkers { get; set; }

    /// <summary>
    ///     已注册任务（按 HandlerType 聚合）
    /// </summary>
    public List<WorkerTaskCoverage> Tasks { get; set; } = [];

    /// <summary>
    ///     没有任何存活节点承载的任务（部署缺失或 HandlerType 配错）
    /// </summary>
    public List<UncoveredJob> UncoveredJobs { get; set; } = [];
}

/// <summary>
///     单个任务在各节点的覆盖情况
/// </summary>
public class WorkerTaskCoverage
{
    /// <summary>
    ///     任务实现类全名（IJob 实现）
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     承载该任务的节点数
    /// </summary>
    public int WorkerCount => Nodes.Count;

    /// <summary>
    ///     承载该任务的节点集合
    /// </summary>
    public List<WorkerNodeRef> Nodes { get; set; } = [];
}

/// <summary>
///     节点引用（报告内使用，避免暴露完整心跳字段）
/// </summary>
public class WorkerNodeRef
{
    /// <summary>
    ///     节点编号
    /// </summary>
    public string WorkerId { get; set; }

    /// <summary>
    ///     节点名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     节点地址
    /// </summary>
    public string Address { get; set; }
}

/// <summary>
///     无节点承载的任务
/// </summary>
public class UncoveredJob
{
    /// <summary>
    ///     任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     任务逻辑名
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     任务实现类全名（可能为空）
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     任务状态
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    ///     下次触发时刻
    /// </summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    ///     原因描述
    /// </summary>
    public string Reason { get; set; }
}
