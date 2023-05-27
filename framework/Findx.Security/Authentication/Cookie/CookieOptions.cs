using Microsoft.Extensions.Options;

namespace Findx.Security.Authentication.Cookie;

/// <summary>
///     Cookie认证配置
/// </summary>
public class CookieOptions : IOptions<CookieOptions>
{
    /// <summary>
    ///     获取或设置 Cookie名称
    /// </summary>
    public string CookieName { get; set; }

    /// <summary>
    ///     获取或设置 登录地址
    /// </summary>
    public string LoginPath { get; set; }

    /// <summary>
    ///     获取或设置 登出地址
    /// </summary>
    public string LogoutPath { get; set; }

    /// <summary>
    ///     获取或设置 无权限地址
    /// </summary>
    public string AccessDeniedPath { get; set; }

    /// <summary>
    ///     获取或设置 返回Url参数名，默认为 ReturnUrl
    /// </summary>
    public string ReturnUrlParameter { get; set; }

    /// <summary>
    ///     获取或设置 过期分钟数
    /// </summary>
    public int ExpireMinutes { get; set; }

    /// <summary>
    ///     获取或设置 是否滑动过期
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    ///     获取或设置 是否启用
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     this
    /// </summary>
    public CookieOptions Value => this;
}