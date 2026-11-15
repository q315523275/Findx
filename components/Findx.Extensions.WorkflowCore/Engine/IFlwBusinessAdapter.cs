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
    ///     读取业务单据锚点（单号/标题/详情地址），返回 null 表示单据不存在
    /// </summary>
    Task<FlwBusinessInfo> LoadAsync(long businessId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发起前校验：返回 false 中止发起并回滚（如单据已作废、金额不满足、重复发起）
    /// </summary>
    Task<bool> OnBeforeStartAsync(FlwBusinessContext context, CancellationToken cancellationToken = default);

    /// <summary>
    ///     实例状态变化：审批中（发起/推进）、通过、拒绝、撤销、超时、终止。
    ///     <para>业务单据状态回写写在这里，与流程推进同一事务</para>
    /// </summary>
    Task OnInstanceStateChangedAsync(FlwBusinessContext context, FlwInstanceState state, CancellationToken cancellationToken = default);
}
