using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Options;

/// <summary>
///     工作流组件配置（配置节 Findx:Workflow）
/// </summary>
public class WorkflowCoreOptions
{
    /// <summary>
    ///     是否启用任务提醒与超时扫描
    ///     <para>缺省关闭：扫描会改动任务数据（提醒时间、超时动作），必须由宿主显式开启</para>
    /// </summary>
    public bool TimeoutEnabled { get; set; }

    /// <summary>
    ///     任务到期后的处理动作（需 TimeoutEnabled = true 才生效）
    /// </summary>
    public FlwTimeoutAction TimeoutAction { get; set; } = FlwTimeoutAction.None;

    /// <summary>
    ///     单次扫描任务数量上限
    /// </summary>
    public int ScanBatchSize { get; set; } = 200;
}
