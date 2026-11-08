using System.Net;
using Findx.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.UnitTests;

/// <summary>
///     XFF 伪造头绕过回归测试
///     <para>修复后语义：默认以直连 RemoteIpAddress 为准；仅当直连对端命中 Findx:HttpRequest:TrustedProxies 白名单时，
///     才从 X-Forwarded-For 右侧剥离可信代理段取客户端IP（不采信最左段，该段客户端可伪造）。</para>
/// </summary>
public class XffTests
{
    private static DefaultHttpContext BuildContext(string remoteIp, string xff, IDictionary<string, string> config)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = remoteIp == null ? null : IPAddress.Parse(remoteIp);
        if (!string.IsNullOrEmpty(xff)) context.Request.Headers["X-Forwarded-For"] = xff;

        if (config != null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(config).Build());
            context.RequestServices = services.BuildServiceProvider();
        }

        return context;
    }

    private static IDictionary<string, string> TrustedProxies(params string[] cidrs)
    {
        var config = new Dictionary<string, string>();
        for (var i = 0; i < cidrs.Length; i++)
            config[$"Findx:HttpRequest:TrustedProxies:{i}"] = cidrs[i];
        return config;
    }

    [Fact]
    public void GetClientIp_Default_IgnoresSpoofedXff()
    {
        // 攻击者伪造 XFF 为 127.0.0.1（绕过内网限制/IP 限速）；默认配置下必须取真实对端
        var context = BuildContext("10.0.0.5", "127.0.0.1", null);

        Assert.Equal("10.0.0.5", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_Default_NoConfig_IgnoresXff()
    {
        var context = BuildContext("10.0.0.5", "8.8.8.8", new Dictionary<string, string>
        {
            ["Other:Key"] = "1"
        });

        Assert.Equal("10.0.0.5", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_RemoteTrusted_ResolvesClientIp()
    {
        // 直连对端 10.0.0.5 命中白名单 → 解析 XFF；最右段 10.0.0.9 也是可信代理 → 继续左移取 1.2.3.4
        var context = BuildContext("10.0.0.5", "1.2.3.4, 10.0.0.9", TrustedProxies("10.0.0.0/8"));

        Assert.Equal("1.2.3.4", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_RemoteNotTrusted_IgnoresXff()
    {
        // 直连对端不在白名单（公网直连）→ 即使伪造 XFF 也不采信
        var context = BuildContext("8.8.8.8", "1.2.3.4", TrustedProxies("10.0.0.0/8"));

        Assert.Equal("8.8.8.8", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_MultiLayer_StripsRightToLeft()
    {
        // 链路：客户端 → 代理A(172.16.0.9) → 代理B(10.0.0.9) → 应用(10.0.0.5)
        // 右侧两段均命中白名单继续剥离，取到非可信的 8.8.8.8
        var context = BuildContext("10.0.0.5", "8.8.8.8, 172.16.0.9, 10.0.0.9", TrustedProxies("10.0.0.0/8", "172.16.0.0/12"));

        Assert.Equal("8.8.8.8", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_AllSegmentsTrusted_FallsBackToRemote()
    {
        // 整条 XFF 都是内网代理（如服务间调用）→ 退回直连对端
        var context = BuildContext("10.0.0.5", "10.0.0.9, 172.16.0.9", TrustedProxies("10.0.0.0/8", "172.16.0.0/12"));

        Assert.Equal("10.0.0.5", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_SkipsInvalidSegment()
    {
        // 垃圾段直接跳过，不把非法内容当IP返回
        var context = BuildContext("10.0.0.5", "1.2.3.4, not-an-ip", TrustedProxies("10.0.0.0/8"));

        Assert.Equal("1.2.3.4", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_HandlesPortSuffix()
    {
        var context = BuildContext("10.0.0.5", "1.2.3.4:5678", TrustedProxies("10.0.0.0/8"));

        Assert.Equal("1.2.3.4", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_SupportsIpv6()
    {
        // 代理地址精确匹配（/128），XFF 中客户端为同段外的 2001:db8::1
        var context = BuildContext("2001:db8::2", "2001:db8::1", TrustedProxies("2001:db8::2/128"));

        Assert.Equal("2001:db8::1", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_Ipv6CidrRange_MatchesProxyButNotClient()
    {
        // /48 代理网段：2001:db8:1::2 命中，客户端 2001:db8:2::1 不命中
        var context = BuildContext("2001:db8:1::2", "2001:db8:2::1", TrustedProxies("2001:db8:1::/48"));

        Assert.Equal("2001:db8:2::1", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_EmptyXff_FallsBackToRemote()
    {
        var context = BuildContext("10.0.0.5", null, TrustedProxies("10.0.0.0/8"));

        Assert.Equal("10.0.0.5", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_CommaSeparatedSingleValue_Works()
    {
        var context = BuildContext("10.0.0.5", "1.2.3.4", new Dictionary<string, string>
        {
            ["Findx:HttpRequest:TrustedProxies"] = "10.0.0.0/8, 172.16.0.0/12"
        });

        Assert.Equal("1.2.3.4", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustForwardedHeaders_NoWhitelist_TakesRightmostSegment()
    {
        // 兼容逃生口：未配置白名单时只取最右段（最近一跳代理追加、单层代理下即客户端IP）；
        // 最左段为客户端可伪造值，不再采信（旧实现取最左，属于可绕过风险）
        var context = BuildContext("10.0.0.5", "8.8.8.8, 7.7.7.7", new Dictionary<string, string>
        {
            ["Findx:HttpRequest:TrustForwardedHeaders"] = "true"
        });

        Assert.Equal("7.7.7.7", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_TrustedProxies_WhitelistWinsOverLegacySwitch()
    {
        // 两者同时配置时以白名单为准（避免旧开关把中间件已清洗的结果再次覆盖成伪造值）
        var config = TrustedProxies("10.0.0.0/8");
        config["Findx:HttpRequest:TrustForwardedHeaders"] = "true";

        var context = BuildContext("10.0.0.5", "8.8.8.8, 10.0.0.9", config);

        Assert.Equal("8.8.8.8", context.GetClientIp());
    }

    [Fact]
    public void GetClientIp_NoRemoteIp_ReturnsEmpty()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        Assert.Equal(string.Empty, context.GetClientIp());
    }
}
