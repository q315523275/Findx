using Findx.Extensions.WorkflowCore.Engine;

namespace Findx.Extensions.WorkflowCore.Dtos.Component;

/// <summary>
///     审批组件面板：一个接口渲染"发起态 / 审批中 / 已完结"三种界面
/// </summary>
public class FlwPanelDto
{
    /// <summary>
    ///     该业务类型是否配置了已发布的流程
    /// </summary>
    public bool HasProcess { get; set; }

    /// <summary>
    ///     是否已发起
    /// </summary>
    public bool Started { get; set; }

    /// <summary>
    ///     当前是否可发起（含已结束单据的重新发起）。
    ///     <para>由业务适配器的 CanStartAsync 判定；未配置流程、审批中、已通过时为 false</para>
    /// </summary>
    public bool CanStart { get; set; }

    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     流程实例编号
    /// </summary>
    public string InstanceNo { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    ///     流程编码
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     业务类型
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务主键
    /// </summary>
    public long BusinessId { get; set; }

    /// <summary>
    ///     发起人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     提交时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     实例状态：-1 未发起；0 审批中 1 通过 2 拒绝 3 撤销 4 超时 5 终止
    /// </summary>
    public int InstanceState { get; set; } = -1;

    /// <summary>
    ///     当前节点名称
    /// </summary>
    public string CurrentNodeName { get; set; }

    /// <summary>
    ///     表单定义 JSON（动态表单场景下发，外挂单据返回 null）
    /// </summary>
    public string FormSchema { get; set; }

    /// <summary>
    ///     标准审批提交的表单数据（存于实例变量 __form__，业务审批为空）
    /// </summary>
    public Dictionary<string, object> FormData { get; set; } = new();

    /// <summary>
    ///     业务锚点（单号/标题/详情地址）
    /// </summary>
    public FlwBusinessInfo Business { get; set; }

    /// <summary>
    ///     业务单据字段值（审批详情只读展示；标准审批或未注册适配器时为空）
    /// </summary>
    public List<FlwBusinessFieldValueDto> BusinessFields { get; set; } = new();

    /// <summary>
    ///     流程定义模型 JSON（审批详情按流程配置只读渲染流程图用；实例态取发起时的模型快照）
    /// </summary>
    public string ModelContent { get; set; }

    /// <summary>
    ///     当前登录人可执行的动作：submit / revoke / approve / reject / transfer / addSign / jump
    /// </summary>
    public List<string> Actions { get; set; } = new();

    /// <summary>
    ///     当前节点的驳回策略：prev 上一节点 / creator 发起人 / specified 指定节点 / terminate 终止流程
    /// </summary>
    public string RejectPolicy { get; set; } = "prev";

    /// <summary>
    ///     驳回策略为 specified 时的目标节点 key
    /// </summary>
    public string RejectTargetKey { get; set; }

    /// <summary>
    ///     当前登录人的待办任务ID（为 0 表示当前无待办）
    /// </summary>
    public long MyTaskId { get; set; }

    /// <summary>
    ///     审批时间轴（节点维度，会签节点按节点合并）
    /// </summary>
    public List<FlwTimelineNodeDto> Timeline { get; set; } = new();

    /// <summary>
    ///     评论列表（存于实例变量 __comments__）
    /// </summary>
    public List<FlwCommentDto> Comments { get; set; } = new();
}

/// <summary>
///     审批评论
/// </summary>
public class FlwCommentDto
{
    /// <summary>
    ///     评论人
    /// </summary>
    public string Actor { get; set; }

    /// <summary>
    ///     评论内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     评论时间
    /// </summary>
    public DateTime? Time { get; set; }
}

/// <summary>
///     业务单据字段值（审批详情只读表单）
/// </summary>
public class FlwBusinessFieldValueDto
{
    /// <summary>
    ///     字段标识
    /// </summary>
    public string Prop { get; set; }

    /// <summary>
    ///     字段名称
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     值类型：number / string / date / bool
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     字段值（日期已格式化为 yyyy-MM-dd）
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
///     时间轴节点
/// </summary>
public class FlwTimelineNodeDto
{
    /// <summary>
    ///     节点 key
    /// </summary>
    public string NodeKey { get; set; }

    /// <summary>
    ///     节点名称
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    ///     节点状态，见 FlwTimelineNodeState
    /// </summary>
    public int State { get; set; }

    /// <summary>
    ///     参与人数（会签/或签总人数）
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    ///     已办理人数
    /// </summary>
    public int Done { get; set; }

    /// <summary>
    ///     办理明细
    /// </summary>
    public List<FlwTimelineActorDto> Actors { get; set; } = new();
}

/// <summary>
///     时间轴办理人
/// </summary>
public class FlwTimelineActorDto
{
    /// <summary>
    ///     办理人名称
    /// </summary>
    public string ActorName { get; set; }

    /// <summary>
    ///     待办理为 -1；已办理取 FlwTaskAction（0 同意 1 拒绝 2 撤销 3 转办 6 跳转 7 自动完成）
    /// </summary>
    public int Action { get; set; }

    /// <summary>
    ///     处理意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     处理时间（待办为空）
    /// </summary>
    public DateTime? Time { get; set; }
}

/// <summary>
///     待办角标
/// </summary>
public class FlwTaskCountDto
{
    /// <summary>
    ///     待办数量
    /// </summary>
    public int TodoCount { get; set; }

    /// <summary>
    ///     已办数量
    /// </summary>
    public int DoneCount { get; set; }

    /// <summary>
    ///     我发起的数量
    /// </summary>
    public int AppliedCount { get; set; }

    /// <summary>
    ///     我收到的数量（我参与但非我发起）
    /// </summary>
    public int ReceivedCount { get; set; }
}

/// <summary>
///     流程实例条目（我的申请 / 我收到的）
/// </summary>
public class FlwInstanceItemDto
{
    /// <summary>
    ///     实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     实例编号
    /// </summary>
    public string InstanceNo { get; set; }

    /// <summary>
    ///     流程编码
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    ///     实例状态：0 审批中 1 通过 2 拒绝 3 撤销 4 超时 5 终止
    /// </summary>
    public int InstanceState { get; set; }

    /// <summary>
    ///     当前节点名称
    /// </summary>
    public string CurrentNodeName { get; set; }

    /// <summary>
    ///     业务类型
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务主键
    /// </summary>
    public long BusinessId { get; set; }

    /// <summary>
    ///     业务单号
    /// </summary>
    public string BusinessCode { get; set; }

    /// <summary>
    ///     业务标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     业务详情地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     发起人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     提交时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     是否可撤销（审批中 + 本人发起）
    /// </summary>
    public bool CanRevoke { get; set; }
}

/// <summary>
///     可发起的流程（发起审批页的磁贴）
/// </summary>
public class FlwProcessOptionDto
{
    /// <summary>
    ///     流程ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     流程编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     说明
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     绑定的业务类型；为空表示可直接在审批中心发起（无业务单据）
    /// </summary>
    public string BusinessType { get; set; }
}

/// <summary>
///     流程分组
/// </summary>
public class FlwProcessGroupDto
{
    /// <summary>
    ///     分组名称
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     分组下的流程
    /// </summary>
    public List<FlwProcessOptionDto> Processes { get; set; } = new();
}

/// <summary>
///     发起审批页数据
/// </summary>
public class FlwStartPageDto
{
    /// <summary>
    ///     顶部统计
    /// </summary>
    public FlwTaskCountDto Counts { get; set; }

    /// <summary>
    ///     最近使用
    /// </summary>
    public List<FlwProcessOptionDto> Recent { get; set; } = new();

    /// <summary>
    ///     按分组排列的流程
    /// </summary>
    public List<FlwProcessGroupDto> Groups { get; set; } = new();
}

/// <summary>
///     待办条目（带业务锚点，便于前端直接跳转业务详情）
/// </summary>
public class FlwTodoItemDto
{
    /// <summary>
    ///     任务ID
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    ///     节点名称
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     流程实例编号
    /// </summary>
    public string InstanceNo { get; set; }

    /// <summary>
    ///     流程名称
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    ///     业务类型
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务主键
    /// </summary>
    public long BusinessId { get; set; }

    /// <summary>
    ///     业务单号
    /// </summary>
    public string BusinessCode { get; set; }

    /// <summary>
    ///     业务标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     业务详情地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     发起人（流程实例创建人）
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     任务创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     本次处理动作（已办有值，待办为 -1）
    /// </summary>
    public int Action { get; set; } = -1;

    /// <summary>
    ///     处理意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     处理完成时间
    /// </summary>
    public DateTime? FinishTime { get; set; }
}

/// <summary>
///     审批组件发起请求
/// </summary>
public class ComponentStartDto
{
    /// <summary>
    ///     业务类型
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务主键
    /// </summary>
    public long BusinessId { get; set; }

    /// <summary>
    ///     业务单号（可按业务适配器返回覆盖）
    /// </summary>
    public string BusinessCode { get; set; }

    /// <summary>
    ///     流程编码（不传则按业务类型匹配已发布流程）
    /// </summary>
    public string ProcessKey { get; set; }

    /// <summary>
    ///     流程变量（条件分支使用，通常由业务适配器补充）
    /// </summary>
    public Dictionary<string, object> Variables { get; set; }

    /// <summary>
    ///     标准审批的动态表单数据（发起时存入实例变量 __form__）
    /// </summary>
    public Dictionary<string, object> FormData { get; set; }
}

/// <summary>
///     审批组件动作请求
/// </summary>
public class ComponentActionDto
{
    /// <summary>
    ///     任务ID
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    ///     动作：approve / reject / transfer / addSign / jump
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    ///     处理意见
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     流程变量（如金额变化后重算分支）
    /// </summary>
    public Dictionary<string, object> Variables { get; set; }

    /// <summary>
    ///     驳回目标节点 / 跳转目标节点
    /// </summary>
    public string TargetNodeKey { get; set; }

    /// <summary>
    ///     驳回时是否直接结束流程
    /// </summary>
    public bool EndProcess { get; set; }

    /// <summary>
    ///     转办目标人
    /// </summary>
    public string TargetActorId { get; set; }

    /// <summary>
    ///     转办目标人名称
    /// </summary>
    public string TargetActorName { get; set; }

    /// <summary>
    ///     加签人员
    /// </summary>
    public List<Models.FlwActorModel> Actors { get; set; }
}

/// <summary>
///     催办请求
/// </summary>
public class ComponentUrgeDto
{
    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     催办留言
    /// </summary>
    public string Comment { get; set; }
}

/// <summary>
///     评论请求
/// </summary>
public class ComponentCommentDto
{
    /// <summary>
    ///     流程实例ID
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     评论内容
    /// </summary>
    public string Content { get; set; }
}
