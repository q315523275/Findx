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

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlowLongEngine(FlwStore store, IUnitOfWorkManager unitOfWorkManager, ILogger<FlowLongEngine> logger, IFlwActorProvider actorProvider = null)
    {
        _store = store;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
        _actorProvider = actorProvider;
    }

    /// <inheritdoc />
    public async Task<FlwInstanceInfo> StartAsync(FlwStartArgs args, CancellationToken cancellationToken = default)
    {
        if (args == null || string.IsNullOrWhiteSpace(args.Creator?.Id)) throw new FlwEngineException("发起人不能为空");

        var process = await _store.GetProcessAsync(args.ProcessKey, args.ProcessId, cancellationToken);
        if (process == null) throw new FlwEngineException($"流程定义不存在：{args.ProcessKey ?? args.ProcessId.ToString()}");
        if (process.Status != 1) throw new FlwEngineException($"流程[{process.Name}]未发布或已停用");

        var model = FlwModelParser.Parse(process.ModelContent, process.Code, process.Name, process.Type);

        await using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<FlwInstanceInfo>(true, cancellationToken);
        _store.AttachUnitOfWork(uow);
        try
        {
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
            await uow.CommitAsync(cancellationToken);
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
                    var sequenceTask = await _store.CreateTaskAsync(execution.Instance, node, new List<FlwTaskActorInfo> { actors[0] }, cancellationToken);
                    await _store.SetSequenceActorsAsync(sequenceTask, actors.Skip(1).ToList(), cancellationToken);
                    return;
                }

                await _store.CreateTaskAsync(execution.Instance, node, actors, cancellationToken);
                return;
            }

            case FlwNodeType.CarbonCopy:
            {
                var actors = await ResolveActorsAsync(execution, node, cancellationToken);
                if (actors.Count > 0) await _store.CreateTaskAsync(execution.Instance, node, actors, cancellationToken);
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
                await _store.CreateTaskAsync(execution.Instance, node, new List<FlwTaskActorInfo>(), cancellationToken);
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
}
