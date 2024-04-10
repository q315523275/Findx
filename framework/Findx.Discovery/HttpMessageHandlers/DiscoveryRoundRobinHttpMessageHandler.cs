using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Findx.Discovery.LoadBalancer;

namespace Findx.Discovery.HttpMessageHandlers
{
    /// <summary>
    /// 轮询请求负载计算Http消息处理器
    /// </summary>
    public class DiscoveryRoundRobinHttpMessageHandler : DelegatingHandler
    {
        private readonly ILoadBalancerProvider _loadBalancerManager;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="loadBalancerManager"></param>
        public DiscoveryRoundRobinHttpMessageHandler(ILoadBalancerProvider loadBalancerManager)
        {
            _loadBalancerManager = loadBalancerManager;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;

            var loadBalancer = await _loadBalancerManager.GetAsync(current.Host, LoadBalancerType.RoundRobin).ConfigureAwait(false);
            var serviceInfo = await loadBalancer.ResolveServiceInstanceAsync().ConfigureAwait(false);
            request.RequestUri = serviceInfo.ToUri(current.Scheme, current.PathAndQuery);

            var nowTime = DateTime.Now;
            try
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                request.RequestUri = current;
                await loadBalancer.UpdateStatsAsync(serviceInfo, DateTime.Now - nowTime);
            }
        }
    }
}