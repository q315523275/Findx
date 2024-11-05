using System.ComponentModel;

namespace Findx.Module.EleAdmin.Shared.Enum;

/// <summary>
///     通用状态 枚举
/// </summary>
public enum CommonStatus
{
    /// <summary>
    ///     正常
    /// </summary>
    [Description("正常")] Enable = 0,

    /// <summary>
    ///     停用
    /// </summary>
    [Description("停用")] Disable = 1
}