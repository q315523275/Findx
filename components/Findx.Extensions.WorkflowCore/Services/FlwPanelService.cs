using Findx.Data;
using Findx.Extensions.WorkflowCore.Dtos.Component;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Storage;

namespace Findx.Extensions.WorkflowCore.Services;

/// <summary>
///     审批组件聚合服务：把"流程定义 + 实例 + 任务 + 业务锚点"拼成一个面板结构，
///     前端一个接口即可渲染发起态 / 审批中 / 已完结三种界面
/// </summary>
public class FlwPanelService
{
    private readonly IRepository<FlwProcessInfo, long> _processRepo;
    private readonly IRepository<FlwInstanceInfo, long> _instanceRepo;
    private readonly FlwStore _store;
    private readonly IFlowLongEngine _engine;
    private readonly FlwBusinessAdapterRegistry _adapters;

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwPanelService(IRepository<FlwProcessInfo, long> processRepo, IRepository<FlwInstanceInfo, long> instanceRepo,
        FlwStore store, IFlowLongEngine engine, FlwBusinessAdapterRegistry adapters = null)
    {
        _processRepo = processRepo;
        _instanceRepo = instanceRepo;
        _store = store;
        _engine = engine;
        _adapters = adapters;
    }

    /// <summary>
    ///     按业务单据获取审批面板
    /// </summary>
    public async Task<FlwPanelDto> GetPanelAsync(string businessType, long businessId, FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var dto = new FlwPanelDto();

        var adapter = _adapters?.Get(businessType);
        if (adapter != null) dto.Business = await adapter.LoadAsync(businessId, cancellationToken);

        var process = await FindProcessAsync(businessType, cancellationToken);
        dto.HasProcess = process != null;
        dto.ProcessName = process?.Name;
        dto.FormSchema = process?.FormSchema;

        var instance = await FindInstanceAsync(businessType, businessId, cancellationToken);
        if (instance == null)
        {
            //  未发起：给节点预览 + 发起动作，前端可以先把流程走线画出来
            dto.Started = false;
            dto.InstanceState = -1;
            if (process != null)
            {
                dto.Actions = new List<string> { "submit" };
                dto.Timeline = await BuildTimelineAsync(ParseNodes(process.ModelContent, process.Code, process.Name, process.Type),
                    new List<FlwHisTaskInfo>(), new List<FlwTaskInfo>(), cancellationToken);
            }
            return dto;
        }

        dto.Started = true;
        dto.InstanceId = instance.Id;
        dto.InstanceNo = instance.InstanceNo;
        dto.InstanceState = (int)instance.InstanceState;
        dto.CurrentNodeName = instance.CurrentNodeName;
        dto.ProcessName = instance.ProcessName ?? dto.ProcessName;

        //  时间轴用实例启动时的模型快照，流程改版不影响在途单据
        var ext = await _store.GetExtAsync(instance.Id, cancellationToken);
        var modelContent = ext?.ModelContent ?? process?.ModelContent;
        var nodes = ParseNodes(modelContent, instance.ProcessKey, instance.ProcessName, ProcessType.Business);

        var hisTasks = await _store.GetHisTasksAsync(instance.Id, cancellationToken);
        var activeTasks = await _store.GetActiveTasksAsync(instance.Id, cancellationToken);
        dto.Timeline = await BuildTimelineAsync(nodes, hisTasks, activeTasks, cancellationToken);

        if (instance.InstanceState == FlwInstanceState.Running)
        {
            var todoTasks = await _engine.GetTodoTasksAsync(actor, 999, cancellationToken);
            var myTask = todoTasks.FirstOrDefault(x => x.InstanceId == instance.Id);
            if (myTask != null)
            {
                dto.MyTaskId = myTask.Id;
                dto.Actions.AddRange(new[] { "approve", "reject", "transfer", "addSign" });
            }
            if (actor != null && instance.CreatorId?.ToString() == actor.Id) dto.Actions.Add("revoke");
        }

        return dto;
    }

    /// <summary>
    ///     待办列表：任务 + 业务锚点，前端可直接跳业务详情
    /// </summary>
    public async Task<List<FlwTodoItemDto>> GetTodoListAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        var result = new List<FlwTodoItemDto>();
        var tasks = await _engine.GetTodoTasksAsync(actor, top, cancellationToken);
        foreach (var task in tasks)
        {
            var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
            if (instance == null) continue;

            var adapter = _adapters?.Get(instance.BusinessType);
            var business = adapter == null ? null : await adapter.LoadAsync(instance.BusinessId, cancellationToken);

            result.Add(new FlwTodoItemDto
            {
                TaskId = task.Id,
                TaskName = task.TaskName,
                InstanceId = instance.Id,
                InstanceNo = instance.InstanceNo,
                ProcessName = instance.ProcessName,
                BusinessType = instance.BusinessType,
                BusinessId = instance.BusinessId,
                BusinessCode = instance.BusinessCode ?? business?.BusinessCode,
                Title = business?.Title,
                Url = business?.Url,
                CreatedTime = task.CreatedTime
            });
        }

        return result;
    }

    /// <summary>
    ///     已办列表：历史任务 + 业务锚点，前端可直接打开审批面板或业务详情
    /// </summary>
    public async Task<List<FlwTodoItemDto>> GetDoneListAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        var result = new List<FlwTodoItemDto>();
        var tasks = await _engine.GetDoneTasksAsync(actor, top, cancellationToken);
        foreach (var task in tasks)
        {
            var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
            if (instance == null) continue;

            var adapter = _adapters?.Get(instance.BusinessType);
            var business = adapter == null ? null : await adapter.LoadAsync(instance.BusinessId, cancellationToken);

            result.Add(new FlwTodoItemDto
            {
                TaskId = task.Id,
                TaskName = task.TaskName,
                InstanceId = instance.Id,
                InstanceNo = instance.InstanceNo,
                ProcessName = instance.ProcessName,
                BusinessType = instance.BusinessType,
                BusinessId = instance.BusinessId,
                BusinessCode = instance.BusinessCode ?? business?.BusinessCode,
                Title = business?.Title,
                Url = business?.Url,
                CreatedTime = task.CreatedTime,
                Action = (int)task.Action,
                Comment = task.Comment,
                FinishTime = task.FinishTime
            });
        }

        return result;
    }

    /// <summary>
    ///     待办 / 已办角标
    /// </summary>
    public async Task<FlwTaskCountDto> GetCountAsync(FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var todo = await _engine.GetTodoTasksAsync(actor, 999, cancellationToken);
        var done = await _engine.GetDoneTasksAsync(actor, 999, cancellationToken);
        return new FlwTaskCountDto { TodoCount = todo.Count, DoneCount = done.Count };
    }

    /// <summary>
    ///     按业务类型找当前生效的流程定义（同编码取最高版本）
    /// </summary>
    public async Task<FlwProcessInfo> FindProcessAsync(string businessType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessType)) return null;
        var list = await _processRepo.SelectAsync(x => x.BusinessType == businessType && x.Status == 1, cancellationToken: cancellationToken);
        return list?.OrderByDescending(x => x.Version).ThenByDescending(x => x.Id).FirstOrDefault();
    }

    /// <summary>
    ///     按业务锚点找实例：优先在途，其次最新
    /// </summary>
    public async Task<FlwInstanceInfo> FindInstanceAsync(string businessType, long businessId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessType) || businessId <= 0) return null;
        var list = await _instanceRepo.SelectAsync(x => x.BusinessType == businessType && x.BusinessId == businessId, cancellationToken: cancellationToken);
        return list?.OrderByDescending(x => x.InstanceState == FlwInstanceState.Running).ThenByDescending(x => x.Id).FirstOrDefault();
    }

    /// <summary>
    ///     解析模型，只取审批与抄送节点（分支容器不展示）
    /// </summary>
    private static List<FlwNodeModel> ParseNodes(string modelContent, string code, string name, ProcessType type)
    {
        if (string.IsNullOrWhiteSpace(modelContent)) return new List<FlwNodeModel>();
        try
        {
            var model = FlwModelParser.Parse(modelContent, code, name, type);
            if (model?.Root == null) return new List<FlwNodeModel>();
            return model.Root.Traverse()
                .Where(x => x.Type == FlwNodeType.Approval || x.Type == FlwNodeType.CarbonCopy)
                .ToList();
        }
        catch
        {
            return new List<FlwNodeModel>();
        }
    }

    /// <summary>
    ///     构建节点维度时间轴
    /// </summary>
    private async Task<List<FlwTimelineNodeDto>> BuildTimelineAsync(List<FlwNodeModel> nodes, List<FlwHisTaskInfo> hisTasks,
        List<FlwTaskInfo> activeTasks, CancellationToken cancellationToken)
    {
        var result = new List<FlwTimelineNodeDto>();
        foreach (var node in nodes)
        {
            var hisNode = hisTasks.Where(x => x.TaskKey == node.NodeKey).ToList();
            var activeNode = activeTasks.Where(x => x.TaskKey == node.NodeKey).ToList();

            var item = new FlwTimelineNodeDto
            {
                NodeKey = node.NodeKey,
                NodeName = node.NodeName,
                State = ResolveNodeState(node, hisNode, activeNode),
                Total = hisNode.Count + activeNode.Count,
                Done = hisNode.Count(x => x.TaskState == FlwTaskState.Completed)
            };

            foreach (var his in hisNode.OrderBy(x => x.FinishTime))
            {
                item.Actors.Add(new FlwTimelineActorDto
                {
                    ActorName = his.LastUpdater,
                    Action = (int)his.Action,
                    Comment = his.Comment,
                    Time = his.FinishTime
                });
            }

            foreach (var active in activeNode)
            {
                var actors = await _store.GetActorsAsync(active.Id, cancellationToken);
                foreach (var actor in actors)
                {
                    //  -1 表示待处理（0 是"同意"，不能复用）
                    item.Actors.Add(new FlwTimelineActorDto { ActorName = actor.ActorName, Action = -1 });
                }
            }

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    ///     节点状态归一：待处理 / 处理中 / 已通过 / 已驳回 / 已跳过 / 抄送
    /// </summary>
    private static int ResolveNodeState(FlwNodeModel node, List<FlwHisTaskInfo> hisTasks, List<FlwTaskInfo> activeTasks)
    {
        if (node.Type == FlwNodeType.CarbonCopy) return (int)FlwTimelineNodeState.CarbonCopy;
        if (activeTasks.Count > 0) return (int)FlwTimelineNodeState.Running;
        if (hisTasks.Count == 0) return (int)FlwTimelineNodeState.Pending;
        if (hisTasks.Any(x => x.TaskState == FlwTaskState.Rejected || x.TaskState == FlwTaskState.RejectTerminated))
            return (int)FlwTimelineNodeState.Rejected;
        if (hisTasks.All(x => x.TaskState == FlwTaskState.Completed || x.TaskState == FlwTaskState.Jump))
            return (int)FlwTimelineNodeState.Approved;
        return (int)FlwTimelineNodeState.Skipped;
    }
}
