using System.Net;
using Findx.Common;
using Findx.WebSocketCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore.Extensions;

/// <summary>
///     扩展
/// </summary>
public static class WebSocketCoreExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public static IApplicationBuilder MapWebSocket(this IApplicationBuilder app, PathString path, WebSocketHandlerBase handler)
    {
        return app.Map(path, x => x.UseMiddleware<WebSocketMiddleware>(handler));
    }

    /// <summary>
    ///     获取客户端IP地址
    /// </summary>
    /// <remarks>
    ///     安全收紧：默认以直连 RemoteIpAddress 为准，不再无条件信任 X-Forwarded-For 首段（该请求头可任意伪造）。
    ///     本扩展无配置入口；如需透传反向代理后的真实客户端IP，请在网关/应用侧启用 ASP.NET Core
    ///     ForwardedHeadersMiddleware 统一处理后再读取 Connection.RemoteIpAddress。
    /// </remarks>
    public static string GetRemoteIpAddress(this HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress;

        var ip = remoteIpAddress?.ToString();
        if (remoteIpAddress is { IsIPv4MappedToIPv6: true }) ip = remoteIpAddress.MapToIPv4().ToString();

        if (string.IsNullOrEmpty(ip) && remoteIpAddress != null && IPAddress.IsLoopback(remoteIpAddress))
            return "127.0.0.1";

        Check.NotNullOrWhiteSpace(ip, nameof(ip));

        return ip;
    }
}