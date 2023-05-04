using Microsoft.AspNetCore.Authorization;

namespace Findx.Security.Authorization;

/// <summary>
///     权限授权配置信息
/// </summary>
public class FunctionRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     授权策略名称
    /// </summary>
    public const string Policy = "FindxPolicy";

    /// <summary>
    ///     单设备登录
    /// </summary>
    public bool SingleDeviceEnabled { set; get; }

    /// <summary>
    ///     校验客户端IP变更
    /// </summary>
    public bool VerifyClientIpChanged { set; get; }
}