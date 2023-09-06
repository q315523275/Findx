using Findx.Common;
using Findx.Discovery.HttpMessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Discovery
{
    /// <summary>
    ///     HttpClientBuilder负载均衡构建扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     添加随机负载均衡
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRandomLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryRandomHttpMessageHandler>();
        }

        /// <summary>
        ///     添加轮询负载均衡
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRoundRobinLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryRoundRobinHttpMessageHandler>();
        }

        /// <summary>
        ///     添加请求压力负载均衡
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddLeastConnectLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryLeastConnectHttpMessageHandler>();
        }
        
        /// <summary>
        ///     添加IpHash负载均衡
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddIpHashLoadBalancer(this IHttpClientBuilder httpClientBuilder)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

            return httpClientBuilder.AddHttpMessageHandler<DiscoveryIpHashHttpMessageHandler>();
        }
    }
}