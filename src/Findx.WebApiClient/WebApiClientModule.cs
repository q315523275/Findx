using Findx.Discovery;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.ComponentModel;

namespace Findx.WebApiClient
{
    [Description("Findx-WebApiClient模块")]
    public class WebApiClientModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 30;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = services.GetConfiguration();

            var webApiFinder = services.GetOrAddTypeFinder<IWebApiFinder>(assemblyFinder => new WebApiFinder(assemblyFinder));
            var types = webApiFinder.FindAll();
            foreach (var type in types)
            {
                var attribute = type.GetAttribute<WebApiClientAttribute>();

                var httpApiBuilder = services.AddHttpApi(type);

                // 配置文件配置
                var section = configuration.GetSection($"Findx:WebApiClient:{type.Name}");
                if (section.Exists())
                    httpApiBuilder.ConfigureHttpApi(section);

                if (attribute.FallbackStatus > 0)
                    httpApiBuilder = httpApiBuilder.AddFallbackPolicy(attribute.FallbackMessage, attribute.FallbackStatus);

                if (attribute.ExceptionsAllowedBeforeBreaking > 0)
                    httpApiBuilder = httpApiBuilder.AddCircuitBreakerPolicy(attribute.ExceptionsAllowedBeforeBreaking, attribute.DurationOfBreak);

                if (attribute.Retry > 0)
                    httpApiBuilder = httpApiBuilder.AddRetryPolicy(attribute.Retry);

                if (attribute.Timeout > 0)
                    httpApiBuilder = httpApiBuilder.AddTimeoutPolicy(attribute.Timeout);

                if (attribute.UseDiscovery)
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
                        default:
                            httpApiBuilder = httpApiBuilder.AddRandomLoadBalancer();
                            break;
                    }
                }
            }

            return services;
        }
    }
}
