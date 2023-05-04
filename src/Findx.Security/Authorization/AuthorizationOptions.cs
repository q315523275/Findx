using Microsoft.Extensions.Options;

namespace Findx.Security.Authorization;

/// <summary>
///     授权配置
/// </summary>
public class AuthorizationOptions : IOptions<AuthorizationOptions>
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { set; get; }

    /// <summary>
    ///     校验客户端IP变更
    /// </summary>
    public bool VerifyClientIpChanged { set; get; }

    /// <summary>
    ///     this
    /// </summary>
    public AuthorizationOptions Value => this;
}