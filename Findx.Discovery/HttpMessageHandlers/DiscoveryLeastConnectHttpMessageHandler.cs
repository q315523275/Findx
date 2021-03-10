using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.HttpMessageHandlers
{
    public class DiscoveryLeastConnectHttpMessageHandler : DelegatingHandler
    {
        private readonly ILoadBalancerManager _loadBalancerManager;

        public DiscoveryLeastConnectHttpMessageHandler(ILoadBalancerManager loadBalancerManager)
        {
            _loadBalancerManager = loadBalancerManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;

            var _loadBalancer = await _loadBalancerManager.GetAsync(current.Host, LoadBalancerType.RoundRobin).ConfigureAwait(false);
            var serviceInfo = await _loadBalancer.ResolveServiceInstanceAsync().ConfigureAwait(false);
            request.RequestUri = serviceInfo.ToUri(current.Scheme, current.PathAndQuery);

            var nowTime = DateTime.Now;
            try
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                request.RequestUri = current;
                await _loadBalancer.UpdateStatsAsync(serviceInfo, (DateTime.Now - nowTime));
            }
        }
    }
}
