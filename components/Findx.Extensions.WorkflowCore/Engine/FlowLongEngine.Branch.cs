using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Storage;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     引擎 - 分支汇聚、实例收尾与节点解析
/// </summary>
public partial class FlowLongEngine
{
    /// <summary>
    ///     节点流转到后继：分支末端做汇聚判定，流程末端结束实例
    /// </summary>
    private async Task AdvanceFromNodeAsync(FlwExecution execution, FlwNodeModel node, CancellationToken cancellationToken)
    {
        if (node == null)
        {
            await CompleteInstanceAsync(execution, FlwInstanceState.Approved, cancellationToken);
            return;
        }

        if (node.ChildNode != null)
        {
            await ExecuteNodeAsync(execution, node.ChildNode, cancellationToken);
            return;
        }

        //  分支末端：条件/路由分支直接进入分支节点的后继；并行/包容分支需等待全部选中分支到达
        if (!string.IsNullOrWhiteSpace(node.BranchParentKey) && !string.IsNullOrWhiteSpace(node.BranchRootKey))
        {
            var parent = FindNode(execution.Model, node.BranchParentKey);
            if (parent != null && parent.Type == FlwNodeType.Condition)
            {
                await ExecuteNodeAsync(execution, parent.ChildNode, cancellationToken);
                return;
            }

            if (parent != null && (parent.Type == FlwNodeType.Parallel || parent.Type == FlwNodeType.Inclusive))
            {
                var joined = GetStringList(execution.Variables, $"__join__{parent.NodeKey}");
                if (!joined.Contains(node.BranchRootKey)) joined.Add(node.BranchRootKey);
                execution.Variables[$"__join__{parent.NodeKey}"] = joined;

                var expected = GetStringList(execution.Variables, $"__branches__{parent.NodeKey}");
                if (expected.Count == 0 || expected.All(joined.Contains))
                {
                    execution.Variables[$"__join__{parent.NodeKey}"] = new List<string>();
                    await ExecuteNodeAsync(execution, parent.ChildNode, cancellationToken);
                }
                return;
            }
        }

        await CompleteInstanceAsync(execution, FlwInstanceState.Approved, cancellationToken);
    }

    /// <summary>
    ///     结束实例：归档全部活动任务、更新实例状态、子流程结束时唤醒父流程
    /// </summary>
    private async Task CompleteInstanceAsync(FlwExecution execution, FlwInstanceState state, CancellationToken cancellationToken)
    {
        if (execution.Instance.InstanceState != FlwInstanceState.Running) return;
        execution.Instance.LastUpdater = execution.Operator?.Name;
        execution.Instance.LastUpdaterId = long.TryParse(execution.Operator?.Id, out var operatorId) ? operatorId : execution.Instance.LastUpdaterId;

        var taskState = state switch
        {
            FlwInstanceState.Approved => FlwTaskState.Completed,
            FlwInstanceState.Rejected => FlwTaskState.Rejected,
            FlwInstanceState.Revoked => FlwTaskState.Revoked,
            FlwInstanceState.Timeout => FlwTaskState.Timeout,
            _ => FlwTaskState.Terminated
        };
        var action = state switch
        {
            FlwInstanceState.Rejected => FlwTaskAction.Reject,
            FlwInstanceState.Revoked => FlwTaskAction.Revoke,
            _ => FlwTaskAction.AutoComplete
        };

        await _store.ArchiveNodeTasksAsync(execution.Instance.Id, null, taskState, action, execution.Operator, null, cancellationToken);
        await _store.FinishInstanceAsync(execution.Instance, state, cancellationToken);

        if (execution.Instance.ParentInstanceId > 0 && state == FlwInstanceState.Approved)
            await AdvanceParentAsync(execution.Instance, cancellationToken);
    }

    /// <summary>
    ///     启动子流程并在父实例上挂起一个触发器任务
    /// </summary>
    private async Task StartChildProcessAsync(FlwExecution execution, FlwNodeModel node, CancellationToken cancellationToken)
    {
        var processKey = node.Props?.CallProcessKey;
        var subProcess = await _store.GetProcessAsync(processKey, 0, cancellationToken);
        if (subProcess == null) throw new FlwEngineException($"子流程定义不存在：{processKey}");

        var subArgs = new FlwStartArgs
        {
            ProcessId = subProcess.Id,
            ProcessKey = subProcess.Code,
            BusinessKey = execution.Instance.BusinessCode,
            Variables = execution.Variables,
            Creator = execution.Operator ?? new FlwCreator(execution.Instance.CreatorId?.ToString(), execution.Instance.Creator)
        };
        var subInstance = await _store.CreateInstanceAsync(subProcess, subProcess.ModelContent, subArgs, execution.Instance.Id, cancellationToken);

        var subModel = FlwModelParser.Parse(subProcess.ModelContent, subProcess.Code, subProcess.Name, subProcess.Type);
        var subExecution = new FlwExecution
        {
            Instance = subInstance,
            Model = subModel,
            Variables = execution.Variables,
            Operator = execution.Operator
        };
        await ExecuteNodeAsync(subExecution, subModel.Root.ChildNode, cancellationToken);
        await _store.SaveInstanceAsync(subInstance, cancellationToken);

        //  父实例挂起：等子流程结束（AdvanceParentAsync）后继续后续节点
        await _store.CreateTaskAsync(execution.Instance, node, new List<FlwTaskActorInfo>(), cancellationToken);
    }

    /// <summary>
    ///     子流程结束后推进父实例
    /// </summary>
    private async Task AdvanceParentAsync(FlwInstanceInfo childInstance, CancellationToken cancellationToken)
    {
        var parentInstance = await _store.GetInstanceAsync(childInstance.ParentInstanceId, cancellationToken);
        if (parentInstance == null || parentInstance.InstanceState != FlwInstanceState.Running) return;

        var ext = await _store.GetExtAsync(parentInstance.Id, cancellationToken);
        if (ext == null) return;

        var model = FlwModelParser.Parse(ext.ModelContent, parentInstance.ProcessKey, parentInstance.ProcessName, ext.ProcessType);
        var execution = new FlwExecution
        {
            Instance = parentInstance,
            Model = model,
            Variables = FlwJsonHelper.DeserializeVariables(parentInstance.Variable),
            Operator = new FlwCreator(parentInstance.CreatorId?.ToString(), parentInstance.Creator)
        };

        var activeTasks = await _store.GetActiveTasksAsync(parentInstance.Id, cancellationToken);
        var callTask = activeTasks.FirstOrDefault(x => x.TaskType == FlwTaskType.Trigger);
        if (callTask == null) return;

        var node = FindNode(model, callTask.TaskKey);
        await _store.ArchiveTaskAsync(callTask, FlwTaskState.Completed, FlwTaskAction.AutoComplete, null, "子流程结束", cancellationToken);
        await AdvanceFromNodeAsync(execution, node, cancellationToken);
        await _store.SaveInstanceAsync(parentInstance, cancellationToken);
    }

    /// <summary>
    ///     按 nodeKey 查找节点
    /// </summary>
    private static FlwNodeModel FindNode(FlwModel model, string nodeKey)
    {
        if (model?.Root == null || string.IsNullOrWhiteSpace(nodeKey)) return null;
        return model.Root.Traverse().FirstOrDefault(x => x.NodeKey == nodeKey);
    }

    /// <summary>
    ///     条件分支选择：按 JSON 定义顺序取第一个成立的；全部不成立落到默认分支
    /// </summary>
    private static FlwNodeModel SelectConditionBranch(FlwNodeModel node, IReadOnlyDictionary<string, object> variables)
    {
        FlwNodeModel defaultBranch = null;
        foreach (var branch in node.GetBranchNodes())
        {
            if (string.IsNullOrWhiteSpace(branch.Props?.Expression))
            {
                defaultBranch ??= branch;
                continue;
            }
            if (FlwConditionEvaluator.Evaluate(branch.Props.Expression, variables)) return branch;
        }
        return defaultBranch;
    }

    /// <summary>
    ///     模型办理人转任务参与者（creator 取实例发起人；superior 依赖组织数据，未接入时跳过）
    /// </summary>
    private async Task<List<FlwTaskActorInfo>> ResolveActorsAsync(FlwExecution execution, FlwNodeModel node, CancellationToken cancellationToken)
    {
        var provider = _actorProvider ?? new DefaultFlwActorProvider();
        var actors = await provider.ResolveAsync(new FlwActorResolveContext
        {
            Instance = execution.Instance,
            Node = node,
            Creator = execution.Operator
        }, cancellationToken);

        return actors ?? new List<FlwTaskActorInfo>();
    }

    private static List<string> GetStringList(Dictionary<string, object> variables, string key)
    {
        if (variables == null || !variables.TryGetValue(key, out var raw) || raw == null) return new List<string>();
        if (raw is List<string> strings) return strings;
        if (raw is IEnumerable<object> objects) return objects.OfType<string>().ToList();
        return new List<string>();
    }
}
