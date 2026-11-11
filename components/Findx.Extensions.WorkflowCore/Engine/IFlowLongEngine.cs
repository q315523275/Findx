using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     FlowLong 风格工作流引擎
/// </summary>
public interface IFlowLongEngine
{
    /// <summary>
    ///     发起流程
    /// </summary>
    Task<FlwInstanceInfo> StartAsync(FlwStartArgs args, CancellationToken cancellationToken = default);

    /// <summary>
    ///     同意（会签/或签按参与类型自动判定是否推进）
    /// </summary>
    Task ApproveAsync(long taskId, FlwCreator actor, string comment = null, Dictionary<string, object> variables = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     拒绝（targetNodeKey 指定回退节点；endProcess=true 直接结束流程）
    /// </summary>
    Task RejectAsync(long taskId, FlwCreator actor, string comment = null, string targetNodeKey = null, bool endProcess = false, CancellationToken cancellationToken = default);

    /// <summary>
    ///     撤销（发起人撤回审批中的流程）
    /// </summary>
    Task RevokeAsync(long instanceId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     转办
    /// </summary>
    Task TransferAsync(long taskId, FlwCreator actor, FlwCreator target, CancellationToken cancellationToken = default);

    /// <summary>
    ///     加签
    /// </summary>
    Task AddSignAsync(long taskId, FlwCreator actor, List<FlwActorModel> actors, CancellationToken cancellationToken = default);

    /// <summary>
    ///     跳转（任意节点跳转到目标节点）
    /// </summary>
    Task JumpAsync(long taskId, FlwCreator actor, string targetNodeKey, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     完成触发器/子流程挂起任务
    /// </summary>
    Task CompleteTriggerAsync(long taskId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     待办任务
    /// </summary>
    Task<List<FlwTaskInfo>> GetTodoTasksAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default);

    /// <summary>
    ///     已办任务
    /// </summary>
    Task<List<FlwHisTaskInfo>> GetDoneTasksAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取实例
    /// </summary>
    Task<FlwInstanceInfo> GetInstanceAsync(long instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取实例详情（活动任务 + 历史任务）
    /// </summary>
    Task<FlwInstanceDetail> GetInstanceDetailAsync(long instanceId, CancellationToken cancellationToken = default);
}

/// <summary>
///     实例详情
/// </summary>
public class FlwInstanceDetail
{
    /// <summary>
    ///     活动实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     活动任务
    /// </summary>
    public List<FlwTaskInfo> ActiveTasks { get; set; }

    /// <summary>
    ///     历史任务
    /// </summary>
    public List<FlwHisTaskInfo> HisTasks { get; set; }
}
