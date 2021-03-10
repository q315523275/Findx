using Findx.Discovery.HttpMessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// HttpClientBuilder负载均衡构建扩展
    /// </summary>
    public static partial class Extensions
    {
        public static IHttpClientBuilder AddRandomLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryRandomHttpMessageHandler>();
        }

        public static IHttpClientBuilder AddRoundRobinLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryRoundRobinHttpMessageHandler>();
        }

        public static IHttpClientBuilder AddLeastConnectLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryLeastConnectHttpMessageHandler>();
        }
    }
}
