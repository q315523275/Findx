using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;
using Findx.UA;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;

namespace Findx.AspNetCore.Extensions;

/// <summary>
///     AspNetCore扩展 - Request
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     确定指定的 HTTP 请求是否为 AJAX 请求。
    /// </summary>
    /// <returns>
    ///     如果指定的 HTTP 请求是 AJAX 请求，则为 true；否则为 false。
    /// </returns>
    /// <param name="request">HTTP 请求。</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="request" /> 参数为 null（在 Visual Basic 中为 Nothing）。</exception>
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        Check.NotNull(request, nameof(request));

        return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal)
               || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
    }

    /// <summary>
    ///     确定指定的 HTTP 请求的 ContextType 是否为 Json 方式
    /// </summary>
    public static bool IsJsonContextType(this HttpRequest request)
    {
        Check.NotNull(request, nameof(request));
        var flag = request.Headers.GetOrDefault("Content-Type").SafeString()
                       .IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1
                   || request.Headers.GetOrDefault("Content-Type").SafeString()
                       .IndexOf("text/json", StringComparison.OrdinalIgnoreCase) > -1;
        if (flag) return true;
        flag = request.Headers.GetOrDefault("Accept").SafeString()
                   .IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1
               || request.Headers.GetOrDefault("Accept").SafeString()
                   .IndexOf("text/json", StringComparison.OrdinalIgnoreCase) > -1;
        return flag;
    }

    /// <summary>
    ///     获取客户端IP地址
    /// </summary>
    public static string GetClientIp(this HttpContext context)
    {
        var ip = context.Request.Headers.GetOrDefault("X-Forwarded-For").SafeString().Split(',')[0];
        if (string.IsNullOrWhiteSpace(ip)) ip = context.Request.Headers.GetOrDefault("REMOTE_ADDR");
        if (string.IsNullOrEmpty(ip))
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress is { IsIPv4MappedToIPv6: true }) ip = remoteIpAddress.MapToIPv4().ToString();

            if (string.IsNullOrEmpty(ip) && remoteIpAddress != null && IPAddress.IsLoopback(remoteIpAddress))
                return "127.0.0.1";
        }

        Check.NotNullOrWhiteSpace(ip, nameof(ip));

        return ip;
    }

    /// <summary>
    ///     获取GetUserAgent
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetUserAgentString(this HttpRequest request)
    {
        return request.Headers.GetOrDefault("User-Agent").SafeString();
    }

    /// <summary>
    ///     获取GetUserAgent
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static UserAgent GetUserAgent(this HttpRequest request)
    {
        var ua = request.Headers.GetOrDefault("User-Agent").SafeString();
        return UserAgentUtility.Parse(ua);
    }
    
    
    /// <summary>
    ///     读取<see cref="HttpRequest"/>的Body为字符串
    /// </summary>
    public static async Task<string> ReadAsStringAsync(this HttpRequest request)
    {
        using var reader = new StreamReader(request.Body);
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    ///     读取<see cref="HttpResponse"/>的Body为字符串
    /// </summary>
    public static async Task<string> ReadAsStringAsync(this HttpResponse response)
    {
        using var reader = new StreamReader(response.Body);
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        return await reader.ReadToEndAsync();
    }
}