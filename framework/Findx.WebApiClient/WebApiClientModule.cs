using System.ComponentModel;
using Findx.Discovery;
using Findx.Discovery.HttpMessageHandlers;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.WebApiClient;

/// <summary>
///     Findx-WebApiClient模块
/// </summary>
[Description("Findx-WebApiClient模块")]
public class WebApiClientModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 50;

    /// <summary>
    ///     模块配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();

        var webApiFinder =
            services.GetOrAddTypeFinder<IWebApiFinder>(assemblyFinder => new WebApiFinder(assemblyFinder));
        var types = webApiFinder.FindAll();
        foreach (var type in types)
        {
            // 客户端配置
            var clientOptions = new WebApiClientOptions();
                
            // HttpApi构建
            var httpApiBuilder = services.AddHttpApi(type);

            // 配置文件配置
            var section = configuration.GetSection($"Findx:WebApiClient:{type.Name}");
            if (section.Exists())
            {
                clientOptions = section.Get<WebApiClientOptions>();
                httpApiBuilder.ConfigureHttpApi(section);
            }

            // json默认控制
            httpApiBuilder.ConfigureHttpApi(x =>
            {
                x.JsonDeserializeOptions.PropertyNameCaseInsensitive = true;
                x.JsonSerializeOptions.PropertyNamingPolicy = null;
            });
            
            // 客户端属性
            var attribute = type.GetAttribute<WebApiClientAttribute>();

            // 降级策略
            if (clientOptions.FallbackStatus.HasValue)
            {
                httpApiBuilder = clientOptions.FallbackStatus.Value > 0
                    ? httpApiBuilder.AddFallbackPolicy(clientOptions.FallbackMessage, clientOptions.FallbackStatus.Value)
                    : httpApiBuilder;
            }
            else if (attribute.FallbackStatus > 0)
            {
                httpApiBuilder = httpApiBuilder.AddFallbackPolicy(attribute.FallbackMessage, attribute.FallbackStatus);
            }

            // 熔断触发次数
            if (clientOptions.ExceptionsAllowedBeforeBreaking.HasValue)
            {
                httpApiBuilder = clientOptions.ExceptionsAllowedBeforeBreaking.Value > 0
                    ? httpApiBuilder.AddCircuitBreakerPolicy(clientOptions.ExceptionsAllowedBeforeBreaking.Value, clientOptions.DurationOfBreak)
                    : httpApiBuilder;
            }
            else if (attribute.ExceptionsAllowedBeforeBreaking > 0)
            {
                httpApiBuilder = httpApiBuilder.AddCircuitBreakerPolicy(attribute.ExceptionsAllowedBeforeBreaking, attribute.DurationOfBreak);
            }

            // 重试策略
            if (clientOptions.Retry.HasValue)
            {
                httpApiBuilder = clientOptions.Retry.Value > 0
                    ? httpApiBuilder.AddRetryPolicy(clientOptions.Retry.Value)
                    : httpApiBuilder;
            }
            else if (attribute.Retry > 0)
            {
                httpApiBuilder = httpApiBuilder.AddRetryPolicy(attribute.Retry);
            }

            // 超时策略
            if (clientOptions.Timeout.HasValue)
            {
                httpApiBuilder = clientOptions.Timeout.Value > 0
                    ? httpApiBuilder.AddTimeoutPolicy(clientOptions.Timeout.Value)
                    : httpApiBuilder;
            }
            else if (attribute.Timeout > 0)
            {
                httpApiBuilder = httpApiBuilder.AddTimeoutPolicy(attribute.Timeout);
            }

            // 服务发现策略
            if (clientOptions.UseDiscovery.HasValue)
            {
                if (clientOptions.UseDiscovery.Value)
                {
                    switch (clientOptions.LoadBalancerType)
                    {
                        case LoadBalancerType.LeastConnection:
                            httpApiBuilder = httpApiBuilder.AddLeastConnectLoadBalancer();
                            break;
                        case LoadBalancerType.Random:
                            httpApiBuilder = httpApiBuilder.AddRandomLoadBalancer();
                            break;
                        case LoadBalancerType.RoundRobin:
                            httpApiBuilder = httpApiBuilder.AddRoundRobinLoadBalancer();
                            break;
                        case LoadBalancerType.IpHash:
                            httpApiBuilder = httpApiBuilder.AddIpHashLoadBalancer();
                            break;
                        default:
                            httpApiBuilder = httpApiBuilder.AddRandomLoadBalancer();
                            break;
                    }
                }
            }
            else if (attribute.UseDiscovery)
            {
                switch (attribute.LoadBalancerType)
                {
                    case LoadBalancerType.LeastConnection:
                        httpApiBuilder = httpApiBuilder.AddLeastConnectLoadBalancer();
                        break;
                    case LoadBalancerType.Random:
                        httpApiBuilder = httpApiBuilder.AddRandomLoadBalancer();
                        break;
                    case LoadBalancerType.RoundRobin:
                        httpApiBuilder = httpApiBuilder.AddRoundRobinLoadBalancer();
                        break;
                    case LoadBalancerType.IpHash:
                        httpApiBuilder = httpApiBuilder.AddIpHashLoadBalancer();
                        break;
                    default:
                        httpApiBuilder = httpApiBuilder.AddRandomLoadBalancer();
                        break;
                }
            }
        }

        return services;
    }
}