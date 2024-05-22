using Findx.Security;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     功能信息
/// </summary>
public class MvcFunction : FunctionBase
{
    /// <summary>
    ///     获取或设置 显示名称
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     获取或设置 路由访问模版
    /// </summary>
    public string RouteTemplate { get; set; }
}