using System.Text.Json;
using Findx.Data;
using Findx.Extensions.WorkflowCore.Dtos.Component;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Storage;
using Findx.Notifications;

namespace Findx.Extensions.WorkflowCore.ServiceDefaults;

/// <summary>
///     审批组件聚合服务：把"流程定义 + 实例 + 任务 + 业务锚点"拼成一个面板结构，
///     前端一个接口即可渲染发起态 / 审批中 / 已完结三种界面
/// </summary>
public class FlwPanelService
{
    private readonly IRepository<FlwProcessInfo, long> _processRepo;
    private readonly IRepository<FlwInstanceInfo, long> _instanceRepo;
    private readonly FlwStore _store;
    private readonly IFlowEngine _engine;
    private readonly FlwBusinessAdapterRegistry _adapters;
    private readonly IUserNotifier _notifier;

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwPanelService(IRepository<FlwProcessInfo, long> processRepo, IRepository<FlwInstanceInfo, long> instanceRepo,
        FlwStore store, IFlowEngine engine, FlwBusinessAdapterRegistry adapters = null, IUserNotifier notifier = null)
    {
        _processRepo = processRepo;
        _instanceRepo = instanceRepo;
        _store = store;
        _engine = engine;
        _adapters = adapters;
        _notifier = notifier;
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
        dto.ProcessKey = process?.Code;
        dto.ProcessName = process?.Name;
        dto.FormSchema = process?.FormSchema;
        dto.ModelContent = process?.ModelContent;
        dto.BusinessType = businessType;
        dto.BusinessId = businessId;
        if (adapter != null) dto.BusinessFields = await BuildBusinessFieldsAsync(adapter, businessId, cancellationToken);

        //  是否可发起由业务适配器判定（如采购单只有草稿/已拒绝才能发起），
        //  避免面板对着已审核单据渲染一个点了就失败的发起入口
        var canStart = process != null && (adapter == null || await adapter.CanStartAsync(businessId, cancellationToken));

        var instance = await FindInstanceAsync(businessType, businessId, cancellationToken);
        if (instance == null)
        {
            //  未发起：给节点预览，前端可以先把流程走线画出来
            dto.Started = false;
            dto.InstanceState = -1;
            dto.CanStart = canStart;
            if (process != null)
            {
                if (canStart) dto.Actions = new List<string> { "submit" };
                dto.Timeline = await BuildTimelineAsync(ParseNodes(process.ModelContent, process.Code, process.Name, process.Type),
                    new List<FlwHisTaskInfo>(), new List<FlwTaskInfo>(), cancellationToken);
            }
            return dto;
        }

        await FillFromInstanceAsync(dto, instance, process, actor, cancellationToken);
        return dto;
    }

    /// <summary>
    ///     按实例获取审批面板
    ///     <para>待办/已办页只知道实例ID，且标准审批没有业务锚点，不能走"业务类型 + 业务主键"这条路</para>
    /// </summary>
    public async Task<FlwPanelDto> GetPanelByInstanceAsync(long instanceId, FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var dto = new FlwPanelDto();
        if (instanceId <= 0) return dto;

        var instance = await _store.GetInstanceAsync(instanceId, cancellationToken);
        if (instance == null) return dto;

        var process = await FindProcessByKeyAsync(instance.ProcessKey, cancellationToken);
        dto.FormSchema = process?.FormSchema;
        await FillFromInstanceAsync(dto, instance, process, actor, cancellationToken);
        return dto;
    }

    /// <summary>
    ///     按实例填充面板：状态、时间轴、可用动作、业务锚点
    /// </summary>
    private async Task FillFromInstanceAsync(FlwPanelDto dto, FlwInstanceInfo instance, FlwProcessInfo process,
        FlwCreator actor, CancellationToken cancellationToken)
    {
        dto.HasProcess = true;
        dto.Started = true;
        dto.InstanceId = instance.Id;
        dto.InstanceNo = instance.InstanceNo;
        dto.InstanceState = (int)instance.InstanceState;
        dto.CurrentNodeName = instance.CurrentNodeName;
        dto.ProcessName = instance.ProcessName ?? dto.ProcessName;
        dto.ProcessKey = instance.ProcessKey;
        dto.Creator = instance.Creator;
        dto.CreatedTime = instance.CreatedTime;
        dto.FormData = ReadFormData(instance);
        dto.Comments = ReadComments(instance);

        //  标准审批没有业务锚点，业务信息留空由前端按动态表单渲染
        if (!instance.BusinessType.IsNullOrWhiteSpace())
        {
            dto.BusinessType = instance.BusinessType;
            dto.BusinessId = instance.BusinessId;
            var adapter = _adapters?.Get(instance.BusinessType);
            if (adapter != null)
            {
                if (dto.Business == null) dto.Business = await adapter.LoadAsync(instance.BusinessId, cancellationToken);
                dto.BusinessFields = await BuildBusinessFieldsAsync(adapter, instance.BusinessId, cancellationToken);
            }
        }

        //  时间轴用实例启动时的模型快照，流程改版不影响在途单据
        var ext = await _store.GetExtAsync(instance.Id, cancellationToken);
        var modelContent = ext?.ModelContent ?? process?.ModelContent;
        dto.ModelContent = modelContent;
        var nodes = ParseNodes(modelContent, instance.ProcessKey, instance.ProcessName, ProcessType.Business);

        var hisTasks = await _store.GetHisTasksAsync(instance.Id, cancellationToken);
        var activeTasks = await _store.GetActiveTasksAsync(instance.Id, cancellationToken);
        dto.Timeline = await BuildTimelineAsync(nodes, hisTasks, activeTasks, cancellationToken);

        //  已结束的实例（通过 / 拒绝 / 撤回 / 终止）若单据仍可发起，允许重新发起：
        //  单据被拒后再无入口是设计缺陷；已通过的实例被「反审核」退回草稿时也要能重新发起
        if (instance.InstanceState is FlwInstanceState.Approved or FlwInstanceState.Rejected
            or FlwInstanceState.Revoked or FlwInstanceState.Terminated)
        {
            var adapter = _adapters?.Get(instance.BusinessType);
            if (adapter != null && instance.BusinessId > 0
                && await adapter.CanStartAsync(instance.BusinessId, cancellationToken))
            {
                dto.CanStart = true;
                dto.Actions.Add("submit");

                //  重新发起走的是"当前生效"的流程定义，节点预览也必须以定义为准：
                //  旧实例快照可能已经被改过（例如新增/去掉了「发起人自选」节点），
                //  拿旧快照渲染会让发起人漏选办理人，直接卡在后端
                var current = await FindProcessAsync(instance.BusinessType, cancellationToken);
                if (current != null && !string.Equals(current.ModelContent, modelContent, StringComparison.Ordinal))
                {
                    dto.ProcessKey = current.Code;
                    dto.ProcessName = current.Name;
                    dto.ModelContent = current.ModelContent;
                    dto.Timeline = await BuildTimelineAsync(
                        ParseNodes(current.ModelContent, current.Code, current.Name, current.Type),
                        new List<FlwHisTaskInfo>(), new List<FlwTaskInfo>(), cancellationToken);
                }
            }
            return;
        }

        if (instance.InstanceState != FlwInstanceState.Running) return;

        //  按实例精确定位「我的」待办任务，避免为了判断动作权限而拉取全量待办
        var myTask = await _store.FindTodoTaskAsync(instance.Id, actor, cancellationToken);
        if (myTask != null)
        {
            dto.MyTaskId = myTask.Id;
            dto.Actions.AddRange(new[] { "approve", "reject", "transfer", "addSign" });
            ApplyNodeSettings(dto, instance, nodes, myTask.TaskKey);
        }
        if (actor != null && instance.CreatorId?.ToString() == actor.Id) dto.Actions.Add("revoke");
    }

    /// <summary>
    ///     读取业务单据字段值：名称/类型取适配器声明的字段元数据，值取适配器当前读取结果
    /// </summary>
    private static async Task<List<FlwBusinessFieldValueDto>> BuildBusinessFieldsAsync(IFlwBusinessAdapter adapter, long businessId,
        CancellationToken cancellationToken)
    {
        var fields = adapter.GetFields();
        if (fields == null || fields.Count == 0) return new List<FlwBusinessFieldValueDto>();

        var values = await adapter.GetVariablesAsync(businessId, cancellationToken) ?? new Dictionary<string, object>();
        var result = new List<FlwBusinessFieldValueDto>(fields.Count);
        foreach (var field in fields)
        {
            values.TryGetValue(field.Prop, out var value);
            result.Add(new FlwBusinessFieldValueDto
            {
                Prop = field.Prop,
                Label = string.IsNullOrWhiteSpace(field.Label) ? field.Prop : field.Label,
                Type = field.Type,
                Value = FormatBusinessValue(value)
            });
        }
        return result;
    }

    /// <summary>
    ///     统一值格式：日期只留 yyyy-MM-dd，其余转字符串，空值给空串由前端显示占位
    /// </summary>
    private static string FormatBusinessValue(object value)
    {
        if (value == null) return string.Empty;
        if (value is DateTime dateTime) return dateTime.ToString("yyyy-MM-dd");
        if (value is decimal decimalValue) return decimalValue.ToString("0.####");
        if (value is double doubleValue) return doubleValue.ToString("0.####");
        if (value is float floatValue) return floatValue.ToString("0.####");
        return Convert.ToString(value) ?? string.Empty;
    }

    /// <summary>
    ///     把当前节点 props.extend 里的设计态配置变成运行期行为：过滤可用动作、下发驳回策略
    /// </summary>
    private static void ApplyNodeSettings(FlwPanelDto dto, FlwInstanceInfo instance, List<FlwNodeModel> nodes, string taskKey)
    {
        var key = string.IsNullOrWhiteSpace(taskKey) ? instance.CurrentNodeCode : taskKey;
        var extend = nodes.FirstOrDefault(x => x.NodeKey == key)?.Props?.Extend;
        if (extend == null || extend.Count == 0) return;

        if (Normalize(Get(extend, "operations")) is Dictionary<string, object> operations)
        {
            if (GetBool(operations, "transfer") == false) dto.Actions.Remove("transfer");
            if (GetBool(operations, "addSign") == false) dto.Actions.Remove("addSign");
            if (GetBool(operations, "rejectBack") == false) dto.Actions.Remove("reject");
        }

        dto.RejectPolicy = GetString(extend, "rejectPolicy") ?? "prev";
        dto.RejectTargetKey = GetString(extend, "rejectTargetKey") ?? string.Empty;
    }

    private static object Get(IDictionary<string, object> source, string key)
    {
        return source != null && source.TryGetValue(key, out var value) ? value : null;
    }

    private static object Normalize(object value)
    {
        return value is JsonElement element ? FlwJsonHelper.ToClrValue(element) : value;
    }

    private static bool? GetBool(IDictionary<string, object> source, string key)
    {
        var value = Normalize(Get(source, key));
        if (value is bool flag) return flag;
        if (value is string text && bool.TryParse(text, out var parsed)) return parsed;
        return null;
    }

    private static string GetString(IDictionary<string, object> source, string key)
    {
        var value = Normalize(Get(source, key));
        return value?.ToString();
    }

    /// <summary>
    ///     催办：给当前在途节点的其他审批人下发提醒（不含催办人自己）
    ///     <para>未接入任何通知通道时走 NullUserNotifier，只记日志不影响调用方</para>
    /// </summary>
    public async Task<int> UrgeAsync(long instanceId, FlwCreator actor, string comment, CancellationToken cancellationToken = default)
    {
        var instance = await _store.GetInstanceAsync(instanceId, cancellationToken);
        if (instance == null || instance.InstanceState != FlwInstanceState.Running) return 0;

        var receivers = new List<string>();
        var activeTasks = await _store.GetActiveTasksAsync(instanceId, cancellationToken);
        foreach (var task in activeTasks)
        {
            var actors = await _store.GetActorsAsync(task.Id, cancellationToken);
            receivers.AddRange(actors.Where(x => !x.ActorId.IsNullOrWhiteSpace()).Select(x => x.ActorId));
        }

        var targets = receivers.Distinct().Where(x => x != actor?.Id).ToList();
        if (targets.Count == 0) return 0;

        if (_notifier != null)
        {
            await _notifier.SendAsync(new UserNotification
            {
                Receivers = targets.ToArray(),
                Title = $"{instance.ProcessName} 待您审批",
                Content = comment.IsNullOrWhiteSpace()
                    ? $"{actor?.Name ?? "发起人"} 催办了【{instance.ProcessName}】，请尽快处理。"
                    : $"{actor?.Name ?? "发起人"} 催办了【{instance.ProcessName}】：{comment}",
                Business = new BusinessAnchor
                {
                    BusinessType = instance.BusinessType,
                    BusinessId = instance.BusinessId.ToString(),
                    BusinessCode = instance.BusinessCode,
                    Title = instance.ProcessName
                }
            }, cancellationToken);
        }

        return targets.Count;
    }

    /// <summary>
    ///     待办列表：任务 + 业务锚点，前端可直接跳业务详情
    /// </summary>
    public async Task<List<FlwTodoItemDto>> GetTodoListAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return await LoadTaskPageAsync(
            async (offset, size, ct) => await _engine.GetTodoTasksAsync(actor, offset, size, ct),
            skip, top, null, cancellationToken);
    }

    /// <summary>
    ///     已办列表：历史任务 + 业务锚点，前端可直接打开审批面板或业务详情
    /// </summary>
    public async Task<List<FlwTodoItemDto>> GetDoneListAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        return await LoadTaskPageAsync(
            async (offset, size, ct) => await _engine.GetDoneTasksAsync(actor, offset, size, ct),
            skip, top,
            (item, task) =>
            {
                if (task is not FlwHisTaskInfo his) return;
                item.Action = (int)his.Action;
                item.Comment = his.Comment;
                item.FinishTime = his.FinishTime;
            },
            cancellationToken);
    }

    /// <summary>
    ///     按页装配任务列表。
    ///     实例已不存在的任务会在装配时被丢弃，因此底层「源偏移」与「有效条数」不等；
    ///     这里统一把 skip / top 定义为**有效行**的偏移与条数：
    ///     先累计取满 skip + top 条有效数据，再截取中间一段，保证前端按已显示条数翻页不会重复或漏行。
    /// </summary>
    private async Task<List<FlwTodoItemDto>> LoadTaskPageAsync(
        Func<int, int, CancellationToken, Task<IEnumerable<FlwTaskInfo>>> fetch,
        int skip,
        int top,
        Action<FlwTodoItemDto, FlwTaskInfo> fill,
        CancellationToken cancellationToken)
    {
        var page = new List<FlwTodoItemDto>();
        if (top <= 0) return page;

        var want = skip + top;
        var offset = 0;
        while (page.Count < want)
        {
            var need = want - page.Count;
            var tasks = (await fetch(offset, need, cancellationToken))?.ToList();
            if (tasks == null || tasks.Count == 0) break;
            offset += tasks.Count;

            foreach (var task in tasks)
            {
                var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
                if (instance == null) continue;

                var adapter = _adapters?.Get(instance.BusinessType);
                var business = adapter == null ? null : await adapter.LoadAsync(instance.BusinessId, cancellationToken);

                var item = new FlwTodoItemDto
                {
                    TaskId = task.Id,
                    TaskName = task.TaskName,
                    InstanceId = instance.Id,
                    InstanceNo = instance.InstanceNo,
                    ProcessName = instance.ProcessName,
                    BusinessType = instance.BusinessType,
                    BusinessId = instance.BusinessId,
                    BusinessCode = instance.BusinessCode ?? business?.BusinessCode,
                    Title = business?.Title ?? SummarizeForm(ReadFormData(instance)),
                    Url = business?.Url,
                    Creator = instance.Creator,
                    CreatedTime = task.CreatedTime
                };
                fill?.Invoke(item, task);
                page.Add(item);
            }

            //  源数据不足一页说明已经取完，再补也是空列表
            if (tasks.Count < need) break;
        }

        return page.Skip(skip).Take(top).ToList();
    }

    /// <summary>
    ///     待办 / 已办 / 我发起 / 我收到 角标
    /// </summary>
    public async Task<FlwTaskCountDto> GetCountAsync(FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var userId = ToUserId(actor);
        //  角标只要数量，走 COUNT 查询，不拉明细再计数
        var todo = await _store.CountTodoAsync(actor, cancellationToken);
        var done = await _store.CountDoneAsync(actor, cancellationToken);
        var applied = userId > 0 ? await _store.CountAppliedAsync(userId, cancellationToken) : 0;
        var received = userId > 0 ? await _store.CountReceivedAsync(actor, userId, cancellationToken) : 0;
        return new FlwTaskCountDto
        {
            TodoCount = (int)todo,
            DoneCount = (int)done,
            AppliedCount = (int)applied,
            ReceivedCount = (int)received
        };
    }

    /// <summary>
    ///     我发起的实例
    /// </summary>
    public async Task<List<FlwInstanceItemDto>> GetAppliedListAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        var userId = ToUserId(actor);
        if (userId <= 0) return [];

        var rows = await _store.QueryAppliedInstancesAsync(userId, skip, top, cancellationToken);
        return await MapInstancesAsync(rows, userId, cancellationToken);
    }

    /// <summary>
    ///     我收到的实例：我参与过（活动/历史任务参与者）且发起人不是我
    /// </summary>
    public async Task<List<FlwInstanceItemDto>> GetReceivedListAsync(FlwCreator actor, int skip = 0, int top = 100, CancellationToken cancellationToken = default)
    {
        var userId = ToUserId(actor);
        if (userId <= 0) return [];

        var rows = await _store.QueryReceivedInstancesAsync(actor, userId, skip, top, cancellationToken);
        return await MapInstancesAsync(rows, userId, cancellationToken);
    }

    /// <summary>
    ///     发起审批页：统计 + 最近使用 + 按分组的流程列表
    /// </summary>
    public async Task<FlwStartPageDto> GetStartPageAsync(FlwCreator actor, CancellationToken cancellationToken = default)
    {
        var result = new FlwStartPageDto { Counts = await GetCountAsync(actor, cancellationToken) };

        //  子流程只能由主流程的 CallProcess 节点调用，不能从发起页独立发起
        var processes = await _processRepo.SelectAsync(
            x => x.Status == 1 && x.Type != ProcessType.Child, cancellationToken: cancellationToken);
        if (processes == null || processes.Count == 0) return result;

        var ordered = processes.OrderBy(x => x.Sort).ThenBy(x => x.Id).ToList();
        foreach (var group in ordered.GroupBy(x => x.GroupName.IsNullOrWhiteSpace() ? "未分组" : x.GroupName))
        {
            result.Groups.Add(new FlwProcessGroupDto
            {
                GroupName = group.Key,
                Processes = group.Select(ToOption).ToList()
            });
        }

        //  最近使用：我最近发起的流程（按流程编码去重）
        var userId = ToUserId(actor);
        if (userId > 0)
        {
            var instances = await _instanceRepo.SelectAsync(x => x.CreatorId == userId, cancellationToken: cancellationToken);
            var recentKeys = instances.OrderByDescending(x => x.Id).Select(x => x.ProcessKey)
                .Where(x => !x.IsNullOrWhiteSpace()).Distinct().Take(4).ToList();
            result.Recent = ordered.Where(x => recentKeys.Contains(x.Code)).Select(ToOption).ToList();
        }

        return result;
    }

    /// <summary>
    ///     可发起的业务单据候选（发起审批页选择业务单据用）
    /// </summary>
    public async Task<List<FlwBusinessInfo>> GetStartCandidatesAsync(string businessType, string keyword, int top = 20, CancellationToken cancellationToken = default)
    {
        var adapter = _adapters?.Get(businessType);
        if (adapter == null) return new List<FlwBusinessInfo>();
        return await adapter.GetStartCandidatesAsync(keyword, top, cancellationToken);
    }

    /// <summary>
    ///     读取标准审批存放在实例变量 __form__ 中的表单数据
    /// </summary>
    public static Dictionary<string, object> ReadFormData(FlwInstanceInfo instance)
    {
        var variables = FlwJsonHelper.DeserializeVariables(instance?.Variable);
        if (variables.TryGetValue("__form__", out var raw) && raw is Dictionary<string, object> map)
            return map;
        return new Dictionary<string, object>();
    }

    /// <summary>
    ///     读取实例变量 __comments__ 里的评论
    /// </summary>
    public static List<FlwCommentDto> ReadComments(FlwInstanceInfo instance)
    {
        var variables = FlwJsonHelper.DeserializeVariables(instance?.Variable);
        if (!variables.TryGetValue("__comments__", out var raw) || raw is not List<object> list)
            return new List<FlwCommentDto>();

        var result = new List<FlwCommentDto>();
        foreach (var item in list)
        {
            if (item is not Dictionary<string, object> map) continue;
            result.Add(new FlwCommentDto
            {
                Actor = map.TryGetValue("actor", out var actor) ? actor?.ToString() : null,
                Content = map.TryGetValue("content", out var content) ? content?.ToString() : null,
                Time = map.TryGetValue("time", out var time) && DateTime.TryParse(time?.ToString(), out var parsed) ? parsed : null
            });
        }
        return result;
    }

    /// <summary>
    ///     追加一条评论（不产生审批任务，只写实例变量）
    /// </summary>
    public async Task AddCommentAsync(long instanceId, FlwCreator actor, string content, CancellationToken cancellationToken = default)
    {
        if (instanceId <= 0 || string.IsNullOrWhiteSpace(content)) return;
        var instance = await _instanceRepo.GetAsync(instanceId, cancellationToken);
        if (instance == null) return;

        var variables = FlwJsonHelper.DeserializeVariables(instance.Variable);
        var comments = variables.TryGetValue("__comments__", out var raw) && raw is List<object> list
            ? new List<object>(list)
            : new List<object>();
        comments.Add(new Dictionary<string, object>
        {
            ["actor"] = actor?.Name ?? "系统",
            ["content"] = content.Trim(),
            ["time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
        variables["__comments__"] = comments;

        var variable = FlwJsonHelper.Serialize(variables);
        await _instanceRepo.UpdateColumnsAsync(x => new FlwInstanceInfo { Variable = variable },
            x => x.Id == instanceId, cancellationToken);
    }

    /// <summary>
    ///     标准审批没有业务适配器，列表标题取表单里第一个有值的字段
    /// </summary>
    private static string SummarizeForm(Dictionary<string, object> formData)
    {
        return formData.Values
            .Select(x => x is Dictionary<string, object> || x is List<object> ? null : x?.ToString())
            .FirstOrDefault(x => !x.IsNullOrWhiteSpace());
    }

    private static long ToUserId(FlwCreator actor)
    {
        return long.TryParse(actor?.Id, out var id) ? id : 0;
    }

    private static FlwProcessOptionDto ToOption(FlwProcessInfo process)
    {
        return new FlwProcessOptionDto
        {
            Id = process.Id,
            Code = process.Code,
            Name = process.Name,
            Icon = process.Icon,
            Comments = process.Comments,
            BusinessType = process.BusinessType
        };
    }

    private async Task<List<FlwInstanceItemDto>> MapInstancesAsync(List<FlwInstanceInfo> instances, long userId, CancellationToken cancellationToken)
    {
        var result = new List<FlwInstanceItemDto>();
        foreach (var instance in instances)
        {
            var adapter = _adapters?.Get(instance.BusinessType);
            var business = adapter == null ? null : await adapter.LoadAsync(instance.BusinessId, cancellationToken);
            var title = business?.Title ?? SummarizeForm(ReadFormData(instance));

            result.Add(new FlwInstanceItemDto
            {
                InstanceId = instance.Id,
                InstanceNo = instance.InstanceNo,
                ProcessKey = instance.ProcessKey,
                ProcessName = instance.ProcessName,
                InstanceState = (int)instance.InstanceState,
                CurrentNodeName = instance.CurrentNodeName,
                BusinessType = instance.BusinessType,
                BusinessId = instance.BusinessId,
                BusinessCode = instance.BusinessCode ?? business?.BusinessCode,
                Title = title,
                Url = business?.Url,
                Creator = instance.Creator,
                CreatedTime = instance.CreatedTime,
                CanRevoke = instance.InstanceState == FlwInstanceState.Running && instance.CreatorId == userId
            });
        }

        return result;
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
    ///     按流程编码找当前生效的流程定义（同编码取最高版本）
    /// </summary>
    public async Task<FlwProcessInfo> FindProcessByKeyAsync(string processKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processKey)) return null;
        var list = await _processRepo.SelectAsync(x => x.Code == processKey && x.Status == 1, cancellationToken: cancellationToken);
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
