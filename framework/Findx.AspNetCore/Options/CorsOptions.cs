namespace Findx.AspNetCore.Options;

/// <summary>
///     Cors配置项目
/// </summary>
public class CorsOptions
{
    /// <summary>
    ///     获取或设置 允许的请求头
    /// </summary>
    public string[] WithHeaders { get; set; } = [];
    
    /// <summary>
    ///     获取或设置 允许的方法
    /// </summary>
    public string[] WithMethods { get; set; } = [];

    /// <summary>
    ///     获取或设置 允许跨域凭据
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    ///     获取或设置 禁止跨域凭据
    /// </summary>
    public bool DisallowCredentials { get; set; }

    /// <summary>
    ///     获取或设置 允许的来源
    /// </summary>
    public string[] WithOrigins { get; set; } = [];
}