namespace Findx.Extensions.WorkflowCore.Dtos.Model;

/// <summary>
///     已注册的业务模块（来自宿主 IFlwBusinessAdapter 实现集合）
/// </summary>
public class BusinessTypeVo
{
    /// <summary>
    ///     业务类型标识，写入 FlwProcessInfo.BusinessType（如 pms.order）
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务模块显示名（适配器未提供时回退为 BusinessType）
    /// </summary>
    public string DisplayName { get; set; }
}
