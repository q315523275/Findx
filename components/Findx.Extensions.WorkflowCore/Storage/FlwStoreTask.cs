using Findx.Data;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Storage;

/// <summary>
///     工作流存储 - 任务归档与查询
/// </summary>
public partial class FlwStore
{
    /// <summary>
    ///     归档单个任务：活动任务与参与者写入历史表后从活动表删除
    /// </summary>
    public async Task ArchiveTaskAsync(FlwTaskInfo task, FlwTaskState state, FlwTaskAction action, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default)
    {
        if (task == null) return;

        var now = DateTime.Now;
        var actors = await GetActorsAsync(task.Id, cancellationToken);

        var hisTask = new FlwHisTaskInfo
        {
            Id = task.Id,
            InstanceId = task.InstanceId,
            ParentTaskId = task.ParentTaskId,
            TaskName = task.TaskName,
            TaskKey = task.TaskKey,
            TaskType = task.TaskType,
            PerformType = task.PerformType,
            ActionUrl = task.ActionUrl,
            Variable = task.Variable,
            AssignorId = task.AssignorId,
            Assignor = task.Assignor,
            ExpireTime = task.ExpireTime,
            RemindTime = task.RemindTime,
            RemindRepeat = task.RemindRepeat,
            Viewed = task.Viewed,
            TenantId = task.TenantId,
            Creator = task.Creator,
            CreatorId = task.CreatorId,
            CreatedTime = task.CreatedTime,
            LastUpdater = actor?.Name ?? task.LastUpdater,
            LastUpdaterId = actor == null ? task.LastUpdaterId : TryParseLong(actor.Id),
            LastUpdatedTime = now,
            FinishTime = now,
            TaskState = state,
            Duration = task.CreatedTime == null
                ? 0
                : (decimal)Math.Round((now - task.CreatedTime.Value).TotalSeconds, 2),
            Comment = comment,
            Action = action
        };
        await Repo<FlwHisTaskInfo>().InsertAsync(hisTask, cancellationToken);

        if (actors.Count > 0)
        {
            var hisActors = actors.Select(x => new FlwHisTaskActorInfo
            {
                InstanceId = x.InstanceId,
                TaskId = x.TaskId,
                ActorId = x.ActorId,
                ActorName = x.ActorName,
                ActorType = x.ActorType,
                Weight = x.Weight,
                AgentId = x.AgentId,
                AgentType = x.AgentType,
                Ext = x.Ext,
                TenantId = x.TenantId,
                Creator = x.Creator,
                CreatorId = x.CreatorId,
                CreatedTime = x.CreatedTime
            }).ToList();
            await Repo<FlwHisTaskActorInfo>().InsertAsync(hisActors, cancellationToken);
            await Repo<FlwTaskActorInfo>().DeleteAsync(x => x.TaskId == task.Id, cancellationToken);
        }

        await Repo<FlwTaskInfo>().DeleteAsync(x => x.Id == task.Id, cancellationToken);
    }

    /// <summary>
    ///     批量归档任务
    /// </summary>
    public async Task ArchiveTasksAsync(IEnumerable<FlwTaskInfo> tasks, FlwTaskState state, FlwTaskAction action, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default)
    {
        foreach (var task in tasks) await ArchiveTaskAsync(task, state, action, actor, comment, cancellationToken);
    }

    /// <summary>
    ///     归档实例下指定节点（为空则全部）的活动任务
    /// </summary>
    public async Task ArchiveNodeTasksAsync(long instanceId, string nodeKey, FlwTaskState state, FlwTaskAction action, FlwCreator actor, string comment = null, CancellationToken cancellationToken = default)
    {
        var tasks = await GetActiveTasksAsync(instanceId, cancellationToken);
        var targets = string.IsNullOrWhiteSpace(nodeKey) ? tasks : tasks.Where(x => x.TaskKey == nodeKey).ToList();
        await ArchiveTasksAsync(targets, state, action, actor, comment, cancellationToken);
    }

    /// <summary>
    ///     替换任务参与者（转办）：旧参与者归档留痕后写入新参与者
    /// </summary>
    public async Task ReplaceActorsAsync(FlwTaskInfo task, List<FlwTaskActorInfo> actors, FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var oldActors = await GetActorsAsync(task.Id, cancellationToken);
        if (oldActors.Count > 0)
        {
            var hisActors = oldActors.Select(x => new FlwHisTaskActorInfo
            {
                InstanceId = x.InstanceId,
                TaskId = x.TaskId,
                ActorId = x.ActorId,
                ActorName = x.ActorName,
                ActorType = x.ActorType,
                Weight = x.Weight,
                AgentId = x.AgentId,
                AgentType = x.AgentType,
                Ext = x.Ext,
                TenantId = x.TenantId,
                Creator = x.Creator,
                CreatorId = x.CreatorId,
                CreatedTime = x.CreatedTime
            }).ToList();
            await Repo<FlwHisTaskActorInfo>().InsertAsync(hisActors, cancellationToken);
            await Repo<FlwTaskActorInfo>().DeleteAsync(x => x.TaskId == task.Id, cancellationToken);
        }

        var now = DateTime.Now;
        foreach (var item in actors)
        {
            item.TaskId = task.Id;
            item.InstanceId = task.InstanceId;
            item.Creator = actor?.Name;
            item.CreatorId = TryParseLong(actor?.Id);
            item.CreatedTime = now;
        }
        await Repo<FlwTaskActorInfo>().InsertAsync(actors, cancellationToken);

        task.Assignor = actor?.Name;
        task.AssignorId = actor?.Id;
        task.LastUpdater = actor?.Name;
        task.LastUpdaterId = TryParseLong(actor?.Id);
        task.LastUpdatedTime = now;
        await Repo<FlwTaskInfo>().UpdateAsync(task, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     追加任务参与者（加签）
    /// </summary>
    public async Task AddActorsAsync(FlwTaskInfo task, List<FlwTaskActorInfo> actors, FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        foreach (var item in actors)
        {
            item.TaskId = task.Id;
            item.InstanceId = task.InstanceId;
            item.Creator = actor?.Name;
            item.CreatorId = TryParseLong(actor?.Id);
            item.CreatedTime = now;
        }
        await Repo<FlwTaskActorInfo>().InsertAsync(actors, cancellationToken);
    }

    /// <summary>
    ///     顺签排队人员在任务变量中的键
    /// </summary>
    public const string SequenceActorsKey = "__sequence_actors__";

    /// <summary>
    ///     保存顺签排队人员（前一人办结后依次放行）
    /// </summary>
    public async Task SetSequenceActorsAsync(FlwTaskInfo task, List<FlwTaskActorInfo> actors, CancellationToken cancellationToken = default)
    {
        var variables = FlwJsonHelper.DeserializeVariables(task.Variable);
        variables[SequenceActorsKey] = actors.Select(x => new Dictionary<string, object>
        {
            ["id"] = x.ActorId,
            ["name"] = x.ActorName,
            ["type"] = (int)x.ActorType,
            ["weight"] = x.Weight
        }).ToList();

        task.Variable = FlwJsonHelper.Serialize(variables);
        await Repo<FlwTaskInfo>().UpdateAsync(task, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     读取顺签排队人员
    /// </summary>
    public static List<FlwTaskActorInfo> GetSequenceActors(FlwTaskInfo task)
    {
        var result = new List<FlwTaskActorInfo>();
        if (task == null) return result;

        var variables = FlwJsonHelper.DeserializeVariables(task.Variable);
        if (!variables.TryGetValue(SequenceActorsKey, out var raw) || raw is not IEnumerable<object> items) return result;

        foreach (var item in items)
        {
            if (item is not Dictionary<string, object> dict) continue;
            result.Add(new FlwTaskActorInfo
            {
                InstanceId = task.InstanceId,
                TaskId = task.Id,
                ActorId = dict.TryGetValue("id", out var id) ? id?.ToString() : null,
                ActorName = dict.TryGetValue("name", out var name) ? name?.ToString() : null,
                ActorType = dict.TryGetValue("type", out var type) && int.TryParse(type?.ToString(), out var typeValue) ? (FlwActorType)typeValue : FlwActorType.User,
                Weight = dict.TryGetValue("weight", out var weight) && int.TryParse(weight?.ToString(), out var weightValue) ? weightValue : 1
            });
        }
        return result;
    }

    /// <summary>
    ///     待办任务查询（按用户、角色、部门匹配参与者）
    /// </summary>
    public async Task<List<FlwTaskInfo>> QueryTodoAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        var result = new List<FlwTaskInfo>();
        if (actor == null) return result;

        var tasks = await Repo<FlwTaskInfo>().SelectAsync(cancellationToken: cancellationToken);
        foreach (var task in tasks.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.Id))
        {
            var actors = await GetActorsAsync(task.Id, cancellationToken);
            if (!actors.Any(x => MatchActor(x, actor))) continue;

            result.Add(task);
            if (result.Count >= top) break;
        }
        return result;
    }

    /// <summary>
    ///     已办任务查询（从历史参与者与历史任务表匹配）
    /// </summary>
    public async Task<List<FlwHisTaskInfo>> QueryDoneAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        var result = new List<FlwHisTaskInfo>();
        if (actor == null) return result;

        var hisActors = await Repo<FlwHisTaskActorInfo>()
            .SelectAsync(x => x.ActorId == actor.Id && x.ActorType == FlwActorType.User, cancellationToken: cancellationToken);
        var taskIds = hisActors.Select(x => x.TaskId).Distinct().Take(top * 3).ToList();
        if (taskIds.Count == 0) return result;

        var hisTasks = await Repo<FlwHisTaskInfo>().SelectAsync(x => taskIds.Contains(x.Id), cancellationToken: cancellationToken);
        return hisTasks.OrderByDescending(x => x.FinishTime).Take(top).ToList();
    }

    /// <summary>
    ///     参与者匹配：用户直配、角色、部门
    /// </summary>
    public static bool MatchActor(FlwTaskActorInfo actor, FlwCreator user)
    {
        if (actor == null || user == null) return false;
        switch (actor.ActorType)
        {
            case FlwActorType.User:
                return !string.IsNullOrWhiteSpace(actor.ActorId) && actor.ActorId == user.Id;
            case FlwActorType.Role:
                return user.RoleIds is { Count: > 0 } && user.RoleIds.Contains(actor.ActorId);
            case FlwActorType.Department:
                return !string.IsNullOrWhiteSpace(actor.ActorId) && actor.ActorId == user.DeptId;
            default:
                return false;
        }
    }
}
