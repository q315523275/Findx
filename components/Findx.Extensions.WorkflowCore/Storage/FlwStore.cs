using Findx.Data;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.WorkflowCore.Storage;

/// <summary>
///     工作流存储（FlowLong 8 张表的读写封装）
/// </summary>
public partial class FlwStore
{
    private readonly IServiceProvider _serviceProvider;
    private IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     绑定工作单元（引擎在一次流程操作内让 8 张表的写入共享同一事务）
    /// </summary>
    public void AttachUnitOfWork(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private IRepository<TEntity, long> Repo<TEntity>() where TEntity : class, IEntity<long>
    {
        var repo = _serviceProvider.GetRequiredService<IRepository<TEntity, long>>();
        if (_unitOfWork != null) repo.UnitOfWork = _unitOfWork;
        return repo;
    }

    #region 流程定义

    /// <summary>
    ///     获取可用的流程定义（按编码取最高版本）
    /// </summary>
    public async Task<FlwProcessInfo> GetProcessAsync(string processKey, long processId = 0, CancellationToken cancellationToken = default)
    {
        var repo = Repo<FlwProcessInfo>();
        if (processId > 0) return await repo.GetAsync(processId, cancellationToken);

        if (string.IsNullOrWhiteSpace(processKey)) return null;

        var list = await repo.SelectAsync(x => x.Code == processKey && x.Status == 1, cancellationToken: cancellationToken);
        return list.OrderByDescending(x => x.Version).ThenByDescending(x => x.Id).FirstOrDefault();
    }

    #endregion

    #region 流程实例

    /// <summary>
    ///     获取活动实例
    /// </summary>
    public Task<FlwInstanceInfo> GetInstanceAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwInstanceInfo>().GetAsync(instanceId, cancellationToken);
    }

    /// <summary>
    ///     获取历史实例（历史实例 Id 与活动实例 Id 一致）
    /// </summary>
    public Task<FlwHisInstanceInfo> GetHisInstanceAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwHisInstanceInfo>().GetAsync(instanceId, cancellationToken);
    }

    /// <summary>
    ///     获取实例的流程模型快照
    /// </summary>
    public Task<FlwExtInstanceInfo> GetExtAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwExtInstanceInfo>().FirstAsync(x => x.InstanceId == instanceId, cancellationToken);
    }

    /// <summary>
    ///     创建流程实例（活动实例 + 历史实例 + 扩展实例三表同时落库）
    /// </summary>
    public async Task<FlwInstanceInfo> CreateInstanceAsync(FlwProcessInfo process, string modelContent, FlwStartArgs args, long parentInstanceId = 0, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var instance = new FlwInstanceInfo
        {
            ProcessId = process.Id,
            ProcessKey = process.Code,
            ProcessName = process.Name,
            InstanceState = FlwInstanceState.Running,
            ParentInstanceId = parentInstanceId,
            Priority = args.Priority,
            InstanceNo = string.IsNullOrWhiteSpace(args.InstanceNo) ? BuildInstanceNo() : args.InstanceNo,
            BusinessCode = args.BusinessKey,
            Variable = FlwJsonHelper.Serialize(args.Variables ?? new Dictionary<string, object>()),
            CurrentNodeName = process.Name,
            CurrentNodeCode = "root",
            ExpireTime = args.ExpireTime,
            Creator = args.Creator?.Name,
            CreatorId = TryParseLong(args.Creator?.Id),
            CreatedTime = now,
            LastUpdater = args.Creator?.Name,
            LastUpdaterId = TryParseLong(args.Creator?.Id),
            LastUpdatedTime = now
        };
        instance.Owner = args.Creator?.Name;
        instance.OwnerId = TryParseLong(args.Creator?.Id);

        await Repo<FlwInstanceInfo>().InsertAsync(instance, cancellationToken);

        var his = new FlwHisInstanceInfo
        {
            Id = instance.Id,
            ProcessId = instance.ProcessId,
            ParentInstanceId = instance.ParentInstanceId,
            Priority = instance.Priority,
            InstanceNo = instance.InstanceNo,
            BusinessCode = instance.BusinessCode,
            Variable = instance.Variable,
            CurrentNodeName = instance.CurrentNodeName,
            CurrentNodeCode = instance.CurrentNodeCode,
            ExpireTime = instance.ExpireTime,
            Status = (int)FlwInstanceState.Running,
            Duration = 0,
            Creator = instance.Creator,
            CreatorId = instance.CreatorId,
            CreatedTime = now,
            LastUpdater = instance.Creator,
            LastUpdaterId = instance.CreatorId,
            LastUpdatedTime = now
        };
        his.Owner = instance.Owner;
        his.OwnerId = instance.OwnerId;
        await Repo<FlwHisInstanceInfo>().InsertAsync(his, cancellationToken);

        var ext = new FlwExtInstanceInfo
        {
            InstanceId = instance.Id,
            ProcessId = process.Id,
            ProcessName = process.Name,
            ProcessType = process.Type,
            ModelContent = modelContent,
            Creator = instance.Creator,
            CreatorId = instance.CreatorId,
            CreatedTime = now,
            LastUpdater = instance.Creator,
            LastUpdaterId = instance.CreatorId,
            LastUpdatedTime = now
        };
        await Repo<FlwExtInstanceInfo>().InsertAsync(ext, cancellationToken);

        return instance;
    }

    /// <summary>
    ///     实例快照：同步活动实例与历史实例的当前节点/变量
    /// </summary>
    public async Task SaveInstanceAsync(FlwInstanceInfo instance, CancellationToken cancellationToken = default)
    {
        instance.LastUpdatedTime = DateTime.Now;
        await Repo<FlwInstanceInfo>().UpdateAsync(instance, cancellationToken: cancellationToken);

        var his = await GetHisInstanceAsync(instance.Id, cancellationToken);
        if (his == null) return;
        his.CurrentNodeName = instance.CurrentNodeName;
        his.CurrentNodeCode = instance.CurrentNodeCode;
        his.Variable = instance.Variable;
        his.LastUpdater = instance.LastUpdater;
        his.LastUpdaterId = instance.LastUpdaterId;
        his.LastUpdatedTime = DateTime.Now;
        await Repo<FlwHisInstanceInfo>().UpdateAsync(his, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     结束流程实例（更新活动实例状态，同时归档历史实例）
    /// </summary>
    public async Task FinishInstanceAsync(FlwInstanceInfo instance, FlwInstanceState state, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        instance.InstanceState = state;
        instance.LastUpdatedTime = now;
        await Repo<FlwInstanceInfo>().UpdateAsync(instance, cancellationToken: cancellationToken);

        var his = await GetHisInstanceAsync(instance.Id, cancellationToken);
        if (his == null) return;
        his.Status = (int)state;
        his.EndTime = now;
        his.Duration = instance.CreatedTime == null
            ? 0
            : (decimal)Math.Round((now - instance.CreatedTime.Value).TotalSeconds, 2);
        his.CurrentNodeName = instance.CurrentNodeName;
        his.CurrentNodeCode = instance.CurrentNodeCode;
        his.Variable = instance.Variable;
        his.LastUpdater = instance.LastUpdater;
        his.LastUpdaterId = instance.LastUpdaterId;
        his.LastUpdatedTime = now;
        await Repo<FlwHisInstanceInfo>().UpdateAsync(his, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     查询实例的历史任务（按完成时间倒序）
    /// </summary>
    public async Task<List<FlwHisTaskInfo>> GetHisTasksAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        var list = await Repo<FlwHisTaskInfo>().SelectAsync(x => x.InstanceId == instanceId, cancellationToken: cancellationToken);
        return list.OrderByDescending(x => x.FinishTime).ToList();
    }

    /// <summary>
    ///     查询实例的活动任务
    /// </summary>
    public Task<List<FlwTaskInfo>> GetActiveTasksAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwTaskInfo>().SelectAsync(x => x.InstanceId == instanceId, cancellationToken: cancellationToken);
    }

    #endregion

    #region 任务

    /// <summary>
    ///     获取活动任务
    /// </summary>
    public Task<FlwTaskInfo> GetTaskAsync(long taskId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwTaskInfo>().GetAsync(taskId, cancellationToken);
    }

    /// <summary>
    ///     获取任务参与者
    /// </summary>
    public Task<List<FlwTaskActorInfo>> GetActorsAsync(long taskId, CancellationToken cancellationToken = default)
    {
        return Repo<FlwTaskActorInfo>().SelectAsync(x => x.TaskId == taskId, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     获取同实例同节点的全部活动任务
    /// </summary>
    public Task<List<FlwTaskInfo>> GetNodeTasksAsync(long instanceId, string nodeKey, CancellationToken cancellationToken = default)
    {
        return Repo<FlwTaskInfo>().SelectAsync(x => x.InstanceId == instanceId && x.TaskKey == nodeKey, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     创建任务及其参与者
    /// </summary>
    public async Task<FlwTaskInfo> CreateTaskAsync(FlwInstanceInfo instance, FlwNodeModel node, List<FlwTaskActorInfo> actors, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var props = node.Props ?? new FlwNodeProps();
        var task = new FlwTaskInfo
        {
            InstanceId = instance.Id,
            TaskName = node.NodeName,
            TaskKey = node.NodeKey,
            TaskType = node.Type switch
            {
                FlwNodeType.CarbonCopy => FlwTaskType.CarbonCopy,
                FlwNodeType.Trigger or FlwNodeType.CallProcess => FlwTaskType.Trigger,
                _ => FlwTaskType.Approval
            },
            PerformType = props.PerformType,
            Variable = instance.Variable,
            ExpireTime = props.ExpireDay > 0 ? now.AddDays(props.ExpireDay) : null,
            RemindTime = ComputeRemindTime(now, props),
            RemindRepeat = props.RemindRepeat,
            Viewed = 0,
            Creator = instance.LastUpdater,
            CreatorId = instance.LastUpdaterId,
            CreatedTime = now,
            LastUpdater = instance.LastUpdater,
            LastUpdaterId = instance.LastUpdaterId,
            LastUpdatedTime = now
        };
        await Repo<FlwTaskInfo>().InsertAsync(task, cancellationToken);

        foreach (var actor in actors)
        {
            actor.TaskId = task.Id;
            actor.InstanceId = instance.Id;
            actor.Creator = instance.LastUpdater;
            actor.CreatorId = instance.LastUpdaterId;
            actor.CreatedTime = now;
        }
        if (actors.Count > 0) await Repo<FlwTaskActorInfo>().InsertAsync(actors, cancellationToken);

        return task;
    }

    #endregion

    private static string BuildInstanceNo()
    {
        return "FLW" + DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static long? TryParseLong(string value)
    {
        return long.TryParse(value, out var result) ? result : null;
    }
}
