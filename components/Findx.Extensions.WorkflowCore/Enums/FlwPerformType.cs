namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     任务参与类型
/// </summary>
public enum FlwPerformType
{
    /// <summary>
    ///     单人办理（一人完成即流转）
    /// </summary>
    Single = 0,

    /// <summary>
    ///     会签（所有参与者全部完成才流转）
    /// </summary>
    Countersign = 1,

    /// <summary>
    ///     或签（任一参与者完成即流转，其余任务自动取消）
    /// </summary>
    OrSign = 2,

    /// <summary>
    ///     顺签（按参与者顺序依次办理）
    /// </summary>
    SequenceSign = 3
}
