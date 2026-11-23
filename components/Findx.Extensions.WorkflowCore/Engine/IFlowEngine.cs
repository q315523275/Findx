using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     工作流引擎
/// </summary>
public interface IFlowEngine
{
    /// <summary>
    ///     发起流程
    /// </summary>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FlwInstanceInfo> StartAsync(FlwStartArgs args, CancellationToken cancellationToken = default);

    /// <summary>
    ///     同意（会签/或签按参与类型自动判定是否推进）
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="comment"></param>
    /// <param name="variables"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ApproveAsync(long taskId, FlwCreator actor, string comment = null, Dictionary<string, object> variables = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     拒绝（targetNodeKey 指定回退节点；endProcess=true 直接结束流程）
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="comment"></param>
    /// <param name="targetNodeKey"></param>
    /// <param name="endProcess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RejectAsync(long taskId, FlwCreator actor, string comment = null, string targetNodeKey = null, bool endProcess = false, CancellationToken cancellationToken = default);

    /// <summary>
    ///     撤销（发起人撤回审批中的流程）
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="actor"></param>
    /// <param name="comment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RevokeAsync(long instanceId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     转办
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="target"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TransferAsync(long taskId, FlwCreator actor, FlwCreator target, CancellationToken cancellationToken = default);

    /// <summary>
    ///     加签
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="actors"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddSignAsync(long taskId, FlwCreator actor, List<FlwActorModel> actors, CancellationToken cancellationToken = default);

    /// <summary>
    ///     跳转（任意节点跳转到目标节点）
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="targetNodeKey"></param>
    /// <param name="comment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task JumpAsync(long taskId, FlwCreator actor, string targetNodeKey, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     强制终止（管理员/业务作废场景，不校验发起人）
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="actor"></param>
    /// <param name="comment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TerminateAsync(long instanceId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     撤销已通过的实例（业务侧反审核用）
    ///     <para>只把实例状态改为「已撤销」，不回调业务适配器 —— 单据状态由调用方自己回写，
    ///     避免同一笔反审核被适配器再改一次；没有实例时返回 false，在途实例会直接报错</para>
    /// </summary>
    /// <param name="businessType">业务类型标识，如 pms.order</param>
    /// <param name="businessId">业务主键</param>
    /// <param name="actor">操作人</param>
    /// <param name="comment">说明</param>
    /// <param name="cancellationToken">取消标记</param>
    Task<bool> RevokeFinishedByBusinessAsync(string businessType, long businessId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     完成触发器/子流程挂起任务
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="actor"></param>
    /// <param name="comment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteTriggerAsync(long taskId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     待办任务
    /// </summary>
    /// <param name="actor">当前操作人</param>
    /// <param name="skip">跳过的条数（分页偏移）</param>
    /// <param name="top">本页条数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<List<FlwTaskInfo>> GetTodoTasksAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default);

    /// <summary>
    ///     已办任务
    /// </summary>
    /// <param name="actor">当前操作人</param>
    /// <param name="skip">跳过的条数（分页偏移）</param>
    /// <param name="top">本页条数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<List<FlwHisTaskInfo>> GetDoneTasksAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取实例
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FlwInstanceInfo> GetInstanceAsync(long instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取实例详情（活动任务 + 历史任务）
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
