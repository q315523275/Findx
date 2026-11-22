using Findx.Data;
using FreeSql;
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
        await InsertAsync(hisTask, cancellationToken);

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
            await InsertAsync(hisActors, cancellationToken);
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
            await InsertAsync(hisActors, cancellationToken);
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
        await InsertAsync(actors, cancellationToken);

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
        await InsertAsync(actors, cancellationToken);
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
    ///     参与者匹配下推为 EXISTS 子查询，一条 SQL 完成过滤 + 排序 + 分页，
    ///     不再全表拉取任务后逐条查参与者（数据量大时是主要瓶颈）。
    /// </summary>
    public Task<List<FlwTaskInfo>> QueryTodoAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return BuildTodoQuery(actor)
            .OrderByDescending(t => t.CreatedTime)
            .OrderByDescending(t => t.Id)
            .Skip(skip)
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     待办数量（角标用，COUNT 查询）
    /// </summary>
    public Task<long> CountTodoAsync(FlwCreator actor, CancellationToken cancellationToken = default)
    {
        return BuildTodoQuery(actor).CountAsync(cancellationToken);
    }

    /// <summary>
    ///     指定实例下「我的」待办任务（审批面板判断可执行动作用，避免拉全量再筛）
    /// </summary>
    public Task<FlwTaskInfo> FindTodoTaskAsync(long instanceId, FlwCreator actor, CancellationToken cancellationToken = default)
    {
        return BuildTodoQuery(actor).Where(t => t.InstanceId == instanceId).FirstAsync(cancellationToken);
    }

    /// <summary>
    ///     已办任务查询（历史参与者匹配 + 历史任务）
    /// </summary>
    public Task<List<FlwHisTaskInfo>> QueryDoneAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return BuildDoneQuery(actor)
            .OrderByDescending(t => t.FinishTime)
            .OrderByDescending(t => t.Id)
            .Skip(skip)
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     已办数量（角标用，COUNT 查询）
    /// </summary>
    public Task<long> CountDoneAsync(FlwCreator actor, CancellationToken cancellationToken = default)
    {
        return BuildDoneQuery(actor).CountAsync(cancellationToken);
    }

    /// <summary>
    ///     我发起的实例（SQL 分页）
    /// </summary>
    public Task<List<FlwInstanceInfo>> QueryAppliedInstancesAsync(long creatorId, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return BuildAppliedQuery(creatorId)
            .OrderByDescending(i => i.Id)
            .Skip(skip)
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     我发起的实例数量
    /// </summary>
    public Task<long> CountAppliedAsync(long creatorId, CancellationToken cancellationToken = default)
    {
        return BuildAppliedQuery(creatorId).CountAsync(cancellationToken);
    }

    /// <summary>
    ///     我收到的实例：我参与过（活动/历史参与者）且发起人不是我（SQL 分页）
    /// </summary>
    public Task<List<FlwInstanceInfo>> QueryReceivedInstancesAsync(FlwCreator actor, long userId, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return BuildReceivedQuery(actor, userId)
            .OrderByDescending(i => i.Id)
            .Skip(skip)
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     我收到的实例数量
    /// </summary>
    public Task<long> CountReceivedAsync(FlwCreator actor, long userId, CancellationToken cancellationToken = default)
    {
        return BuildReceivedQuery(actor, userId).CountAsync(cancellationToken);
    }

    /* ---------- 查询条件构造：列表、计数、单条定位共用同一套条件 ---------- */

    private ISelect<FlwTaskInfo> BuildTodoQuery(FlwCreator actor)
    {
        //  匹配条件用「不可能命中的占位值」兜底，保证 = / IN 子句始终合法
        var userId = actor == null || string.IsNullOrWhiteSpace(actor.Id) ? NoMatch : actor.Id;
        var deptId = actor == null || string.IsNullOrWhiteSpace(actor.DeptId) ? NoMatch : actor.DeptId;
        var roleIds = (actor?.RoleIds ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (roleIds.Count == 0) roleIds.Add(NoMatch);

        return Fsql.Select<FlwTaskInfo>()
            //  实例已删除的任务不参与列表与计数，避免角标数量与列表条数不一致
            .Where(t => Fsql.Select<FlwInstanceInfo>().Where(i => i.Id == t.InstanceId).Any())
            .Where(t => Fsql.Select<FlwTaskActorInfo>()
                .Where(a => a.TaskId == t.Id
                            && ((a.ActorType == FlwActorType.User && a.ActorId == userId)
                                || (a.ActorType == FlwActorType.Department && a.ActorId == deptId)
                                || (a.ActorType == FlwActorType.Role && roleIds.Contains(a.ActorId))))
                .Any());
    }

    private ISelect<FlwHisTaskInfo> BuildDoneQuery(FlwCreator actor)
    {
        var userId = actor == null || string.IsNullOrWhiteSpace(actor.Id) ? NoMatch : actor.Id;
        return Fsql.Select<FlwHisTaskInfo>()
            //  同待办：实例已删除的历史任务不计入
            .Where(t => Fsql.Select<FlwInstanceInfo>().Where(i => i.Id == t.InstanceId).Any())
            .Where(t => Fsql.Select<FlwHisTaskActorInfo>()
                .Where(a => a.TaskId == t.Id && a.ActorType == FlwActorType.User && a.ActorId == userId)
                .Any());
    }

    private ISelect<FlwInstanceInfo> BuildAppliedQuery(long creatorId)
    {
        return Fsql.Select<FlwInstanceInfo>().Where(i => i.CreatorId == creatorId);
    }

    private ISelect<FlwInstanceInfo> BuildReceivedQuery(FlwCreator actor, long userId)
    {
        var actorId = actor == null || string.IsNullOrWhiteSpace(actor.Id) ? NoMatch : actor.Id;
        return Fsql.Select<FlwInstanceInfo>()
            //  发起人为空的实例（系统创建）按原逻辑视为「不是我发起的」
            .Where(i => i.CreatorId == null || i.CreatorId != userId)
            .Where(i => Fsql.Select<FlwTaskActorInfo>()
                            .Where(a => a.InstanceId == i.Id && a.ActorId == actorId).Any()
                        || Fsql.Select<FlwHisTaskActorInfo>()
                            .Where(a => a.InstanceId == i.Id && a.ActorId == actorId).Any());
    }

    /// <summary>
    ///     参与者匹配的条件占位值：确保生成的条件不可能命中任何真实数据
    /// </summary>
    private const string NoMatch = "__flw_no_match__";

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
