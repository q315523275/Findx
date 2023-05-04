using Microsoft.Extensions.Options;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     Jwt认证配置
/// </summary>
public class JwtOptions : IOptions<JwtOptions>
{
    /// <summary>
    ///     获取或设置 密钥
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    ///     获取或设置 发行方
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    ///     获取或设置 订阅方
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    ///     获取或设置 AccessToken有效期分钟数
    /// </summary>
    public double AccessExpireMinutes { get; set; }

    /// <summary>
    ///     获取或设置 RefreshToken有效期分钟数
    /// </summary>
    public double RefreshExpireMinutes { get; set; }

    /// <summary>
    ///     获取或设置 RefreshToken是否绝对过期
    /// </summary>
    public bool IsRefreshAbsoluteExpired { get; set; } = true;

    /// <summary>
    ///     获取或设置 开始续期分钟数
    /// </summary>
    public double RenewalMinutes { get; set; }

    /// <summary>
    ///     获取或设置 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// </summary>
    public JwtOptions Value => this;
}