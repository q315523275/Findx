using Findx.Data;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     业务单据锚点信息
/// </summary>
public class FlwBusinessInfo
{
    /// <summary>
    ///     业务类型标识，如 pms.order
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
    ///     业务标题（审批组件展示用）
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     业务详情地址（支持 {businessId} 占位，审批组件与待办跳转使用）
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
///     业务单据上下文
/// </summary>
public class FlwBusinessContext
{
    /// <summary>
    ///     流程实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     业务锚点
    /// </summary>
    public FlwBusinessInfo Business { get; set; }

    /// <summary>
    ///     当前工作单元：业务回写请用 <c>repo.WithUnitOfWork(UnitOfWork)</c>，与流程推进同事务
    /// </summary>
    public IUnitOfWork UnitOfWork { get; set; }

    /// <summary>
    ///     当前操作人
    /// </summary>
    public FlwCreator Operator { get; set; }

    /// <summary>
    ///     本次操作的审批意见（发起或终态动作时由引擎透传，可能为空）
    /// </summary>
    public string Comment { get; set; }
}

/// <summary>
///     业务单据字段（供条件分支设计器选择，以及发起审批时写入流程变量）
/// </summary>
public class FlwBusinessField
{
    /// <summary>
    ///     字段标识：写入流程变量时使用的 key（小驼峰，与前端表单字段口径一致）
    /// </summary>
    public string Prop { get; set; }

    /// <summary>
    ///     字段名称（设计器下拉展示）
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     值类型：number / string / date / bool，设计器可据此推荐运算符
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
///     业务单据适配器：让流程"知道"单据在哪、什么状态、如何回写。
///     <para>按 <see cref="BusinessType" /> 注册多个实现即可外挂多类业务单据，不注册则流程与业务互不影响</para>
///     <para>所有回调都在引擎事务内同步执行，抛异常会回滚整个操作</para>
/// </summary>
public interface IFlwBusinessAdapter
{
    /// <summary>
    ///     业务类型标识，与 <see cref="FlwProcessInfo.BusinessType" /> 对应
    /// </summary>
    string BusinessType { get; }

    /// <summary>
    ///     业务模块显示名，审批管理「基础信息」选择业务模块时展示；不实现则回退为 <see cref="BusinessType" />
    /// </summary>
    string DisplayName => BusinessType;

    /// <summary>
    ///     读取业务单据锚点（单号/标题/详情地址），返回 null 表示单据不存在
    /// </summary>
    Task<FlwBusinessInfo> LoadAsync(long businessId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发起前校验：返回 false 中止发起并回滚（如单据已作废、金额不满足、重复发起）
    /// </summary>
    Task<bool> OnBeforeStartAsync(FlwBusinessContext context, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当前单据是否处于"可发起"状态（审批面板据此决定要不要给发起入口）。
    ///     <para>与 <see cref="OnBeforeStartAsync" /> 的区别：这里只做只读判断、不带工作单元，
    ///     供面板预览使用；默认返回 true，即由 <see cref="OnBeforeStartAsync" /> 在真正发起时兜底</para>
    /// </summary>
    Task<bool> CanStartAsync(long businessId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    ///     实例状态变化：审批中（发起/推进）、通过、拒绝、撤销、超时、终止。
    ///     <para>业务单据状态回写写在这里，与流程推进同一事务</para>
    ///     <para>注意：只在「发起」与「实例终结」两个时刻各回调一次，中间节点审批不会触发</para>
    /// </summary>
    Task OnInstanceStateChangedAsync(FlwBusinessContext context, FlwInstanceState state, CancellationToken cancellationToken = default);

    /// <summary>
    ///     任务办理完成：办理人「同意」或「拒绝」后回调一次，实例可能仍在审批中。
    ///     <para>与 <see cref="OnInstanceStateChangedAsync" /> 的区别：这是节点级钩子，每办完一个人就触发，
    ///     适合以后接节点级通知（抄送、催办、节点耗时统计、外部消息推送）；单据状态回写仍走实例级回调</para>
    ///     <para>或签时其余任务被自动归档、实例结束时批量归档的任务都不回调，避免一个人同意推出多条通知</para>
    ///     <para>默认空实现：业务侧暂不需要节点级处理时可以不实现，后续要接通知模块再覆盖即可</para>
    /// </summary>
    /// <param name="context">业务上下文（含实例、单据锚点、工作单元、操作人、本次审批意见）</param>
    /// <param name="task">刚办理完成的任务（含节点标识 TaskKey 与节点名 TaskName）</param>
    /// <param name="action">办理动作：同意 / 拒绝</param>
    /// <param name="cancellationToken">取消标记</param>
    Task OnTaskCompletedAsync(FlwBusinessContext context, FlwTaskInfo task, FlwTaskAction action, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     可发起的业务单据（发起审批页选择用）。
    ///     <para>默认返回空，表示该业务类型不支持在审批中心直接发起，需要到业务模块内发起</para>
    /// </summary>
    Task<List<FlwBusinessInfo>> GetStartCandidatesAsync(string keyword, int top, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<FlwBusinessInfo>());
    }

    /// <summary>
    ///     业务单据可用的条件字段清单（流程设计器条件分支下拉用）。
    ///     <para>默认返回空：业务类型未声明字段时，设计器仍可手工填写字段名</para>
    /// </summary>
    IReadOnlyList<FlwBusinessField> GetFields()
    {
        return Array.Empty<FlwBusinessField>();
    }

    /// <summary>
    ///     读取业务单据当前字段值，发起审批时并入流程实例变量，供条件分支求值。
    ///     <para>默认返回空字典；key 必须与 <see cref="GetFields" /> 的 Prop 一致</para>
    /// </summary>
    Task<Dictionary<string, object>> GetVariablesAsync(long businessId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<string, object>());
    }
}
