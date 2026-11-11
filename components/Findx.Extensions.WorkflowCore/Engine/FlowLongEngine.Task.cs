using Findx.Data;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Storage;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     引擎 - 任务处理（同意/拒绝/撤销/转办/加签/跳转/触发器回调）
/// </summary>
public partial class FlowLongEngine
{
    /// <inheritdoc />
    public Task ApproveAsync(long taskId, FlwCreator actor, string comment = null, Dictionary<string, object> variables = null, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            var context = await LoadTaskContextAsync(taskId, actor, cancellationToken);
            var execution = context.Execution;

            if (variables is { Count: > 0 })
            {
                foreach (var item in variables) execution.Variables[item.Key] = item.Value;
                execution.Instance.Variable = FlwJsonHelper.Serialize(execution.Variables);
            }

            var task = context.Task;
            var sequenceRest = FlwStore.GetSequenceActors(task);
            task.Viewed = 1;
            await _store.ArchiveTaskAsync(task, FlwTaskState.Completed, FlwTaskAction.Agree, actor, comment, cancellationToken);

            var node = FindNode(execution.Model, task.TaskKey);
            var remaining = (await _store.GetNodeTasksAsync(execution.Instance.Id, task.TaskKey, cancellationToken))
                .Where(x => x.TaskType == FlwTaskType.Approval)
                .ToList();

            var shouldAdvance = remaining.Count == 0;
            if (task.PerformType == FlwPerformType.OrSign && remaining.Count > 0)
            {
                //  或签：一人办理即流转，其余任务自动完成
                await _store.ArchiveTasksAsync(remaining, FlwTaskState.Completed, FlwTaskAction.AutoComplete, actor, "或签-他人已处理", cancellationToken);
                shouldAdvance = true;
            }
            else if (task.PerformType == FlwPerformType.SequenceSign && sequenceRest.Count > 0)
            {
                //  顺签：给下一位办理人放行，其余继续排队
                var nextTask = await _store.CreateTaskAsync(execution.Instance, node, new List<FlwTaskActorInfo> { sequenceRest[0] }, cancellationToken);
                if (sequenceRest.Count > 1) await _store.SetSequenceActorsAsync(nextTask, sequenceRest.Skip(1).ToList(), cancellationToken);
                shouldAdvance = false;
            }

            if (shouldAdvance) await AdvanceFromNodeAsync(execution, node, cancellationToken);
            await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task RejectAsync(long taskId, FlwCreator actor, string comment = null, string targetNodeKey = null, bool endProcess = false, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            var context = await LoadTaskContextAsync(taskId, actor, cancellationToken);
            var execution = context.Execution;
            var task = context.Task;

            await _store.ArchiveNodeTasksAsync(execution.Instance.Id, task.TaskKey, FlwTaskState.Rejected, FlwTaskAction.Reject, actor, comment, cancellationToken);

            if (endProcess)
            {
                await CompleteInstanceAsync(execution, FlwInstanceState.Rejected, cancellationToken);
                await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
                return;
            }

            var target = string.IsNullOrWhiteSpace(targetNodeKey)
                ? await FindPreviousNodeAsync(execution, task.TaskKey, cancellationToken)
                : FindNode(execution.Model, targetNodeKey);

            if (target == null)
            {
                await CompleteInstanceAsync(execution, FlwInstanceState.Rejected, cancellationToken);
            }
            else
            {
                await ExecuteNodeAsync(execution, target, cancellationToken);
            }
            await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task RevokeAsync(long instanceId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            var instance = await _store.GetInstanceAsync(instanceId, cancellationToken);
            if (instance == null) throw new FlwEngineException($"流程实例不存在：{instanceId}");
            if (instance.InstanceState != FlwInstanceState.Running) throw new FlwEngineException("流程实例已结束，不能撤销");
            if (actor == null || instance.CreatorId?.ToString() != actor.Id) throw new FlwEngineException("仅发起人可撤销流程");

            var execution = await BuildExecutionAsync(instance, actor, cancellationToken);
            await CompleteInstanceAsync(execution, FlwInstanceState.Revoked, cancellationToken);
            await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task TransferAsync(long taskId, FlwCreator actor, FlwCreator target, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            if (target == null || string.IsNullOrWhiteSpace(target.Id)) throw new FlwEngineException("转办目标人不能为空");

            var context = await LoadTaskContextAsync(taskId, actor, cancellationToken);
            var task = context.Task;
            var newActors = new List<FlwTaskActorInfo>
            {
                new()
                {
                    TaskId = task.Id,
                    InstanceId = task.InstanceId,
                    ActorId = target.Id,
                    ActorName = target.Name,
                    ActorType = FlwActorType.User,
                    Weight = 1
                }
            };
            await _store.ReplaceActorsAsync(task, newActors, actor, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task AddSignAsync(long taskId, FlwCreator actor, List<FlwActorModel> actors, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            if (actors == null || actors.Count == 0) throw new FlwEngineException("加签人员不能为空");

            var context = await LoadTaskContextAsync(taskId, actor, cancellationToken);
            var task = context.Task;
            var items = actors.Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => new FlwTaskActorInfo
            {
                TaskId = task.Id,
                InstanceId = task.InstanceId,
                ActorId = x.Id,
                ActorName = x.Name,
                ActorType = string.Equals(x.Type, "role", StringComparison.OrdinalIgnoreCase) ? FlwActorType.Role
                    : string.Equals(x.Type, "dept", StringComparison.OrdinalIgnoreCase) ? FlwActorType.Department
                    : FlwActorType.User,
                Weight = x.Weight <= 0 ? 1 : x.Weight
            }).ToList();
            if (items.Count == 0) throw new FlwEngineException("加签人员缺少标识");

            await _store.AddActorsAsync(task, items, actor, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task JumpAsync(long taskId, FlwCreator actor, string targetNodeKey, string comment = null, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            var context = await LoadTaskContextAsync(taskId, actor, cancellationToken);
            var execution = context.Execution;
            var target = FindNode(execution.Model, targetNodeKey);
            if (target == null) throw new FlwEngineException($"跳转目标节点不存在：{targetNodeKey}");

            await _store.ArchiveNodeTasksAsync(execution.Instance.Id, context.Task.TaskKey, FlwTaskState.Jump, FlwTaskAction.Jump, actor, comment, cancellationToken);
            await ExecuteNodeAsync(execution, target, cancellationToken);
            await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task CompleteTriggerAsync(long taskId, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(async () =>
        {
            var task = await _store.GetTaskAsync(taskId, cancellationToken);
            if (task == null) throw new FlwEngineException($"任务不存在或已处理：{taskId}");
            if (task.TaskType != FlwTaskType.Trigger) throw new FlwEngineException("该任务不是触发器任务");

            var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
            if (instance == null || instance.InstanceState != FlwInstanceState.Running) throw new FlwEngineException("流程实例已结束");

            var execution = await BuildExecutionAsync(instance, actor, cancellationToken);
            await _store.ArchiveTaskAsync(task, FlwTaskState.Completed, FlwTaskAction.AutoComplete, actor, comment, cancellationToken);
            await AdvanceFromNodeAsync(execution, FindNode(execution.Model, task.TaskKey), cancellationToken);
            await _store.SaveInstanceAsync(execution.Instance, cancellationToken);
        }, cancellationToken);
    }

    private async Task<FlwEngineContext> LoadTaskContextAsync(long taskId, FlwCreator actor, CancellationToken cancellationToken)
    {
        if (actor == null || string.IsNullOrWhiteSpace(actor.Id)) throw new FlwEngineException("操作人不能为空");

        var task = await _store.GetTaskAsync(taskId, cancellationToken);
        if (task == null) throw new FlwEngineException($"任务不存在或已处理：{taskId}");

        var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
        if (instance == null || instance.InstanceState != FlwInstanceState.Running) throw new FlwEngineException("流程实例已结束");

        var actors = await _store.GetActorsAsync(taskId, cancellationToken);
        if (!actors.Any(x => FlwStore.MatchActor(x, actor))) throw new FlwEngineException("当前用户无权处理该任务");

        var execution = await BuildExecutionAsync(instance, actor, cancellationToken);
        return new FlwEngineContext { Task = task, Instance = instance, Actors = actors, Execution = execution };
    }

    private async Task<FlwExecution> BuildExecutionAsync(FlwInstanceInfo instance, FlwCreator actor, CancellationToken cancellationToken)
    {
        var ext = await _store.GetExtAsync(instance.Id, cancellationToken);
        if (ext == null || string.IsNullOrWhiteSpace(ext.ModelContent)) throw new FlwEngineException($"实例[{instance.InstanceNo}]缺少流程模型快照");

        var model = FlwModelParser.Parse(ext.ModelContent, instance.ProcessKey, instance.ProcessName, ext.ProcessType);
        return new FlwExecution
        {
            Instance = instance,
            Model = model,
            Variables = FlwJsonHelper.DeserializeVariables(instance.Variable),
            Operator = actor
        };
    }

    private async Task<FlwNodeModel> FindPreviousNodeAsync(FlwExecution execution, string currentNodeKey, CancellationToken cancellationToken)
    {
        var hisTasks = await _store.GetHisTasksAsync(execution.Instance.Id, cancellationToken);
        var previous = hisTasks.FirstOrDefault(x => x.TaskKey != currentNodeKey && x.TaskType == FlwTaskType.Approval);
        return previous == null ? null : FindNode(execution.Model, previous.TaskKey);
    }

    private async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        await using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<FlwInstanceInfo>(true, cancellationToken);
        _store.AttachUnitOfWork(uow);
        try
        {
            await action();
            await uow.CommitAsync(cancellationToken);
        }
        catch
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

/// <summary>
///     任务处理上下文
/// </summary>
internal class FlwEngineContext
{
    public FlwTaskInfo Task { get; set; }

    public FlwInstanceInfo Instance { get; set; }

    public List<FlwTaskActorInfo> Actors { get; set; }

    public FlwExecution Execution { get; set; }
}
