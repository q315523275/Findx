using Findx.Data;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Storage;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     FlowLong 风格工作流引擎（树形节点驱动，无回溯线；回溯靠驳回/跳转/路由分支）
/// </summary>
public partial class FlowLongEngine: IFlowLongEngine
{
    private readonly FlwStore _store;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<FlowLongEngine> _logger;
    private readonly IFlwActorProvider _actorProvider;
    private readonly FlwBusinessAdapterRegistry _adapterRegistry;
    private readonly IEnumerable<IFlwInstanceListener> _instanceListeners;
    private readonly IFlwTaskNotifier _taskNotifier;

    //  一次流程操作内的提交后回调（通知等副作用），与引擎事务解耦
    private FlwPostCommitQueue _postCommit = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlowLongEngine(FlwStore store, IUnitOfWorkManager unitOfWorkManager, ILogger<FlowLongEngine> logger,
        IFlwActorProvider actorProvider = null, FlwBusinessAdapterRegistry adapterRegistry = null,
        IEnumerable<IFlwInstanceListener> instanceListeners = null, IFlwTaskNotifier taskNotifier = null)
    {
        _store = store;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
        _actorProvider = actorProvider;
        _adapterRegistry = adapterRegistry;
        _instanceListeners = instanceListeners;
        _taskNotifier = taskNotifier;
    }

    /// <inheritdoc />
    public async Task<FlwInstanceInfo> StartAsync(FlwStartArgs args, CancellationToken cancellationToken = default)
    {
        if (args == null || string.IsNullOrWhiteSpace(args.Creator?.Id)) throw new FlwEngineException("发起人不能为空");

        var process = await _store.GetProcessAsync(args.ProcessKey, args.ProcessId, cancellationToken);
        if (process == null) throw new FlwEngineException($"流程定义不存在：{args.ProcessKey ?? args.ProcessId.ToString()}");
        if (process.Status != 1) throw new FlwEngineException($"流程[{process.Name}]未发布或已停用");

        var model = FlwModelParser.Parse(process.ModelContent, process.Code, process.Name, process.Type);

        _postCommit = new FlwPostCommitQueue();

        await using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<FlwInstanceInfo>(true, cancellationToken);
        _store.AttachUnitOfWork(uow);

        //  业务类型显式传入优先，否则取流程定义上的
        var businessType = string.IsNullOrWhiteSpace(args.BusinessType) ? process.BusinessType : args.BusinessType;
        var adapter = _adapterRegistry?.Get(businessType);
        if (adapter != null && args.BusinessId <= 0)
            throw new FlwEngineException($"流程[{process.Name}]已绑定业务类型[{businessType}]，发起时业务主键不能为空");

        try
        {
            FlwBusinessInfo business = null;
            if (adapter != null)
            {
                business = await adapter.LoadAsync(args.BusinessId, cancellationToken);
                var checkContext = new FlwBusinessContext { Business = business, UnitOfWork = uow, Operator = args.Creator };
                var allowed = await adapter.OnBeforeStartAsync(checkContext, cancellationToken);
                if (!allowed) throw new FlwEngineException($"业务单据[{args.BusinessId}]不满足发起条件");
                business ??= new FlwBusinessInfo { BusinessType = businessType, BusinessId = args.BusinessId, BusinessCode = args.BusinessKey };
            }

            var instance = await _store.CreateInstanceAsync(process, process.ModelContent, args, 0, cancellationToken);
            var execution = new FlwExecution
            {
                Instance = instance,
                Model = model,
                Variables = args.Variables ?? new Dictionary<string, object>(),
                Operator = args.Creator
            };

            await ExecuteNodeAsync(execution, model.Root.ChildNode, cancellationToken);
            await _store.SaveInstanceAsync(instance, cancellationToken);

            //  发起成功：业务侧在此把单据置为「待审核」，与实例落库同一事务
            if (adapter != null)
            {
                var context = new FlwBusinessContext { Instance = instance, Business = business, UnitOfWork = uow, Operator = args.Creator };
                await adapter.OnInstanceStateChangedAsync(context, FlwInstanceState.Running, cancellationToken);
            }

            await uow.CommitAsync(cancellationToken);
            await _postCommit.FlushAsync(_logger, cancellationToken);
            return instance;
        }
        catch
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    ///     节点驱动：按节点类型创建任务或继续流转
    /// </summary>
    private async Task ExecuteNodeAsync(FlwExecution execution, FlwNodeModel node, CancellationToken cancellationToken)
    {
        if (execution.Instance.InstanceState != FlwInstanceState.Running) return;

        if (node == null)
        {
            await CompleteInstanceAsync(execution, FlwInstanceState.Approved, cancellationToken);
            return;
        }

        execution.Instance.CurrentNodeName = node.NodeName;
        execution.Instance.CurrentNodeCode = node.NodeKey;

        switch (node.Type)
        {
            case FlwNodeType.Root:
                await ExecuteNodeAsync(execution, node.ChildNode, cancellationToken);
                return;

            case FlwNodeType.Approval:
            {
                var actors = await ResolveActorsAsync(execution, node, cancellationToken);
                if (actors.Count == 0)
                {
                    if (node.Props?.AutoApprove == true)
                    {
                        _logger?.LogDebug("节点[{Node}]未解析到办理人，按 autoApprove 自动通过", node.NodeName);
                        await ExecuteNodeAsync(execution, node.ChildNode, cancellationToken);
                        return;
                    }
                    throw new FlwEngineException($"审批节点[{node.NodeName}]未解析到办理人");
                }

                //  顺签：只为第一人建任务，其余人员排在任务上，前一人办结后再依次放行
                if (node.Props?.PerformType == FlwPerformType.SequenceSign && actors.Count > 1)
                {
                    var sequenceActors = new List<FlwTaskActorInfo> { actors[0] };
                    var sequenceTask = await _store.CreateTaskAsync(execution.Instance, node, sequenceActors, cancellationToken);
                    await _store.SetSequenceActorsAsync(sequenceTask, actors.Skip(1).ToList(), cancellationToken);
                    QueueTaskNotify(execution.Instance, sequenceTask, sequenceActors);
                    return;
                }

                var task = await _store.CreateTaskAsync(execution.Instance, node, actors, cancellationToken);
                QueueTaskNotify(execution.Instance, task, actors);
                return;
            }

            case FlwNodeType.CarbonCopy:
            {
                var actors = await ResolveActorsAsync(execution, node, cancellationToken);
                if (actors.Count > 0)
                {
                    var ccTask = await _store.CreateTaskAsync(execution.Instance, node, actors, cancellationToken);
                    QueueTaskNotify(execution.Instance, ccTask, actors);
                }
                await ExecuteNodeAsync(execution, node.ChildNode, cancellationToken);
                return;
            }

            case FlwNodeType.Condition:
            {
                var branch = SelectConditionBranch(node, execution.Variables);
                if (branch == null) throw new FlwEngineException($"条件分支[{node.NodeName}]未命中任何分支");
                await ExecuteNodeAsync(execution, branch.ChildNode ?? branch, cancellationToken);
                return;
            }

            case FlwNodeType.Parallel:
            case FlwNodeType.Inclusive:
            {
                var branches = node.GetBranchNodes();
                var selected = node.Type == FlwNodeType.Parallel
                    ? branches
                    : branches.Where(x => FlwConditionEvaluator.Evaluate(x.Props?.Expression, execution.Variables)).ToList();
                if (selected.Count == 0) throw new FlwEngineException($"分支节点[{node.NodeName}]未命中任何分支");

                execution.Variables[$"__branches__{node.NodeKey}"] = selected.Select(x => x.NodeKey).ToList();
                execution.Variables[$"__join__{node.NodeKey}"] = new List<string>();
                foreach (var branch in selected) await ExecuteNodeAsync(execution, branch.ChildNode ?? branch, cancellationToken);
                return;
            }

            case FlwNodeType.Route:
            {
                var branch = node.GetBranchNodes().FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.Props?.TargetNodeKey) && FlwConditionEvaluator.Evaluate(x.Props?.Expression, execution.Variables));
                var target = branch == null ? node.ChildNode : FindNode(execution.Model, branch.Props.TargetNodeKey);
                await ExecuteNodeAsync(execution, target ?? node.ChildNode, cancellationToken);
                return;
            }

            case FlwNodeType.Trigger:
            {
                //  触发器任务由外部（定时器/HTTP 回调）完成后调用 CompleteTriggerAsync 继续
                var triggerTask = await _store.CreateTaskAsync(execution.Instance, node, new List<FlwTaskActorInfo>(), cancellationToken);
                QueueTaskNotify(execution.Instance, triggerTask, null);
                return;
            }

            case FlwNodeType.CallProcess:
                await StartChildProcessAsync(execution, node, cancellationToken);
                return;

            default:
                await ExecuteNodeAsync(execution, node.ChildNode, cancellationToken);
                return;
        }
    }

    /// <summary>
    ///     任务产生通知：入提交后队列，事务回滚则不会发出
    /// </summary>
    private void QueueTaskNotify(FlwInstanceInfo instance, FlwTaskInfo task, List<FlwTaskActorInfo> actors)
    {
        if (_taskNotifier == null || task == null) return;
        _postCommit.Add(token => _taskNotifier.NotifyAsync(new FlwTaskNotifyContext
        {
            Instance = instance,
            Task = task,
            Actors = actors ?? new List<FlwTaskActorInfo>(),
            Expired = false
        }, token));
    }

    /// <summary>
    ///     实例状态变化：同步回调业务适配器（引擎事务内）
    /// </summary>
    private async Task NotifyInstanceStateChangedAsync(FlwExecution execution, FlwInstanceState state, CancellationToken cancellationToken)
    {
        var adapter = _adapterRegistry?.Get(execution.Instance?.BusinessType);
        if (adapter == null) return;

        var business = await adapter.LoadAsync(execution.Instance.BusinessId, cancellationToken);
        var context = new FlwBusinessContext
        {
            Instance = execution.Instance,
            Business = business,
            UnitOfWork = _store.CurrentUnitOfWork,
            Operator = execution.Operator,
            Comment = execution.LastComment
        };
        await adapter.OnInstanceStateChangedAsync(context, state, cancellationToken);
    }

    /// <summary>
    ///     实例完结通知：入提交后队列，失败只记日志
    /// </summary>
    private void QueueInstanceFinished(FlwExecution execution, FlwInstanceState state)
    {
        if (_instanceListeners == null || execution?.Instance == null) return;
        var listeners = _instanceListeners.ToList();
        if (listeners.Count == 0) return;

        var instance = execution.Instance;
        var duration = instance.CreatedTime == null
            ? 0
            : (decimal)Math.Round((DateTime.Now - instance.CreatedTime.Value).TotalSeconds, 2);

        _postCommit.Add(async token =>
        {
            var adapter = _adapterRegistry?.Get(instance.BusinessType);
            var business = adapter == null ? null : await adapter.LoadAsync(instance.BusinessId, token);
            foreach (var listener in listeners)
            {
                try
                {
                    await listener.OnInstanceFinishedAsync(new FlwInstanceFinishedContext
                    {
                        Instance = instance,
                        Business = business,
                        State = state,
                        Duration = duration
                    }, token);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "流程实例[{InstanceId}]完结监听器执行失败", instance.Id);
                }
            }
        });
    }
}
