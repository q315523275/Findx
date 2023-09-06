using System;
using Findx.Common;
using Findx.Discovery;
using Findx.Discovery.LoadBalancer;
using WebApiClientCore.Attributes;

namespace Findx.WebApiClient;

[AttributeUsage(AttributeTargets.Interface)]
public class WebApiClientAttribute : HttpHostAttribute
{
    public WebApiClientAttribute(string host) : base(host)
    {
        Check.NotNullOrWhiteSpace(host, nameof(host));
    }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     超时时间
    /// </summary>
    public int Timeout { set; get; } = 60;

    /// <summary>
    ///     重试次数
    /// </summary>
    public int Retry { set; get; }

    /// <summary>
    ///     降级状态
    /// </summary>
    public int FallbackStatus { set; get; }

    /// <summary>
    ///     降级描述
    /// </summary>
    public object FallbackMessage { set; get; }

    /// <summary>
    ///     熔断前连续错误次数
    /// </summary>
    public int ExceptionsAllowedBeforeBreaking { set; get; }

    /// <summary>
    ///     熔断时长,如:30s
    /// </summary>
    public string DurationOfBreak { set; get; }

    /// <summary>
    ///     使用服务发现
    /// </summary>
    public bool UseDiscovery { set; get; }

    /// <summary>
    ///     负载均衡器名称
    /// </summary>
    public LoadBalancerType LoadBalancerType { set; get; } = LoadBalancerType.Random;
}