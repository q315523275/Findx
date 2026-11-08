using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;
using Findx.Utilities;
using Findx.Utilities.UA;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    ///     可信代理网段（CIDR）解析结果缓存：key 为规范化后的配置串，避免每次请求都重复解析
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, CidrRange[]> TrustedProxyRangesCache = new(StringComparer.Ordinal);

    /// <summary>
    ///     获取客户端IP地址
    ///     <para>默认只认直连对端（Connection.RemoteIpAddress）。X-Forwarded-For / REMOTE_ADDR 均可被客户端任意伪造，
    ///     仅当直连对端命中 Findx:HttpRequest:TrustedProxies 白名单（或显式开启 TrustForwardedHeaders）时才解析转发头。</para>
    ///     <para>解析 X-Forwarded-For 时从右往左剥离：跳过白名单内的代理地址，取第一个不可信地址，避免采信最左段（伪造值）。</para>
    /// </summary>
    public static string GetClientIp(this HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        if (remoteIpAddress == null) return string.Empty;
        remoteIpAddress = NormalizeIpAddress(remoteIpAddress);

        //  RequestServices 在真实请求管线中非空；单元测试/非请求上下文可能为 null，此时按默认不信任转发头
        var httpRequestSection = context.RequestServices?.GetService<IConfiguration>()?.GetSection("Findx:HttpRequest");

        //  白名单模式（推荐）：只有直连对端属于可信代理，才解析 X-Forwarded-For
        var trustedProxies = GetTrustedProxyRanges(httpRequestSection);
        if (trustedProxies.Length > 0)
        {
            if (!IsTrustedProxy(trustedProxies, remoteIpAddress)) return remoteIpAddress.ToString();

            var forwardedClientIp = ResolveClientIpFromForwardedFor(context, trustedProxies);
            return forwardedClientIp ?? remoteIpAddress.ToString();
        }

        //  兼容逃生口：显式声明"位于可信反代之后"但未配置网段时，仅取 X-Forwarded-For 最右段
        //  （最近一跳代理追加的地址；单层代理下即客户端IP。最左段是客户端可伪造值，不再采信）
        var trustForwardedHeaders = httpRequestSection?.GetValue<bool>("TrustForwardedHeaders") ?? false;
        if (trustForwardedHeaders)
        {
            var forwardedClientIp = ResolveClientIpFromForwardedFor(context, null);
            if (forwardedClientIp != null) return forwardedClientIp;
        }

        return remoteIpAddress.ToString();
    }

    /// <summary>
    ///     从 X-Forwarded-For 解析客户端IP：从右往左跳过可信代理段，返回第一个不可信地址
    /// </summary>
    private static string ResolveClientIpFromForwardedFor(HttpContext context, CidrRange[] trustedProxies)
    {
        var forwardedFor = context.Request.Headers.GetOrDefault("X-Forwarded-For").SafeString();
        if (string.IsNullOrWhiteSpace(forwardedFor)) return null;

        var segments = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var i = segments.Length - 1; i >= 0; i--)
        {
            //  非法段直接跳过，避免垃圾值/注入内容被当作IP返回
            if (!TryParseIpAddress(segments[i], out var ipAddress)) continue;

            ipAddress = NormalizeIpAddress(ipAddress);

            if (trustedProxies != null && IsTrustedProxy(trustedProxies, ipAddress)) continue;

            return ipAddress.ToString();
        }

        return null;
    }

    /// <summary>
    ///     解析 X-Forwarded-For 中的单个地址段，兼容 "IPv4"、"IPv6"、"[IPv6]:port"、"IPv4:port"
    /// </summary>
    private static bool TryParseIpAddress(string value, out IPAddress ipAddress)
    {
        ipAddress = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        if (IPAddress.TryParse(value, out ipAddress)) return true;

        //  [IPv6]:port
        if (value.Length > 2 && value[0] == '[')
        {
            var end = value.IndexOf(']');
            if (end > 2) return IPAddress.TryParse(value.Substring(1, end - 1), out ipAddress);
        }

        //  IPv4:port
        var lastColon = value.LastIndexOf(':');
        if (lastColon > 0 && int.TryParse(value.Substring(lastColon + 1), out _))
            return IPAddress.TryParse(value.Substring(0, lastColon), out ipAddress);

        return false;
    }

    /// <summary>
    ///     规范化IP：IPv4映射的IPv6转回IPv4，回环统一为 127.0.0.1
    /// </summary>
    private static IPAddress NormalizeIpAddress(IPAddress ipAddress)
    {
        if (ipAddress.IsIPv4MappedToIPv6) ipAddress = ipAddress.MapToIPv4();
        if (IPAddress.IsLoopback(ipAddress)) return IPAddress.Loopback;
        return ipAddress;
    }

    /// <summary>
    ///     读取可信代理网段配置：支持 JSON 数组与逗号分隔字符串两种写法
    /// </summary>
    private static CidrRange[] GetTrustedProxyRanges(IConfigurationSection httpRequestSection)
    {
        var section = httpRequestSection?.GetSection("TrustedProxies");
        if (section == null) return Array.Empty<CidrRange>();

        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(section.Value))
            values.AddRange(section.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        foreach (var child in section.GetChildren())
            if (!string.IsNullOrWhiteSpace(child.Value)) values.Add(child.Value.Trim());

        if (values.Count == 0) return Array.Empty<CidrRange>();

        values.Sort(StringComparer.Ordinal);
        var cacheKey = string.Join("|", values);

        return TrustedProxyRangesCache.GetOrAdd(cacheKey, static key => ParseCidrRanges(key.Split('|')));
    }

    /// <summary>
    ///     解析 CIDR 配置项（无法解析的项静默跳过，保证配置写错不阻断请求）
    /// </summary>
    private static CidrRange[] ParseCidrRanges(string[] values)
    {
        var ranges = new List<CidrRange>(values.Length);
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (TryParseCidr(value.Trim(), out var range)) ranges.Add(range);
        }

        return ranges.ToArray();
    }

    /// <summary>
    ///     解析 CIDR（如 10.0.0.0/8、127.0.0.1/32、::1/128），未带前缀长度时按单地址处理
    /// </summary>
    private static bool TryParseCidr(string value, out CidrRange range)
    {
        range = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var parts = value.Split('/', 2);
        if (!IPAddress.TryParse(parts[0].Trim(), out var network)) return false;
        network = NormalizeIpAddress(network);

        var maxPrefixLength = network.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
        var prefixLength = maxPrefixLength;
        if (parts.Length == 2 && (!int.TryParse(parts[1].Trim(), out prefixLength) || prefixLength < 0 || prefixLength > maxPrefixLength))
            return false;

        range = new CidrRange(network, prefixLength);
        return true;
    }

    /// <summary>
    ///     判断IP是否属于任一 CIDR 网段
    /// </summary>
    private static bool IsTrustedProxy(CidrRange[] ranges, IPAddress ipAddress)
    {
        var ipBytes = ipAddress.GetAddressBytes();

        foreach (var range in ranges)
        {
            if (range.NetworkBytes.Length != ipBytes.Length) continue;
            if (IsInRange(ipBytes, range.NetworkBytes, range.PrefixLength)) return true;
        }

        return false;
    }

    /// <summary>
    ///     按前缀长度做字节掩码比较
    /// </summary>
    private static bool IsInRange(byte[] ipBytes, byte[] networkBytes, int prefixLength)
    {
        var fullBytes = prefixLength / 8;

        for (var i = 0; i < fullBytes; i++)
            if (ipBytes[i] != networkBytes[i]) return false;

        var remainderBits = prefixLength % 8;
        if (remainderBits == 0) return true;

        var mask = (byte)(0xFF << (8 - remainderBits));
        return (ipBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }

    /// <summary>
    ///     CIDR 网段
    /// </summary>
    private readonly struct CidrRange
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        public CidrRange(IPAddress network, int prefixLength)
        {
            NetworkBytes = network.GetAddressBytes();
            PrefixLength = prefixLength;
        }

        /// <summary>
        ///     网段地址字节（未做掩码清零，比较时按前缀长度掩码）
        /// </summary>
        public byte[] NetworkBytes { get; }

        /// <summary>
        ///     前缀长度
        /// </summary>
        public int PrefixLength { get; }
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
        //  开启请求体缓冲，使其可重复 Seek；leaveOpen=true 不关闭 Body，避免影响下游模型绑定
        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    ///     读取<see cref="HttpResponse"/>的Body为字符串
    /// </summary>
    public static async Task<string> ReadAsStringAsync(this HttpResponse response)
    {
        if (response.Body.CanSeek) response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}