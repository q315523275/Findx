using Findx.Discovery.Consul;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Discovery
{
    public abstract class DiscoveryModuleBase : FindxModule
    {
        /// <summary>
        /// 获取 模块级别，级别越小越先启动
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            Configuration = services.GetConfiguration();
            var section = Configuration.GetSection("Findx:Discovery");
            services.Configure<DiscoveryOptions>(section);

            services.TryAddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
            services.TryAddSingleton<ILoadBalancerProvider, LoadBalancerProvider>();
            services.TryAddSingleton<IDiscoveryClient, DiscoveryClient>();

            services.TryAddTransient<DiscoveryLeastConnectHttpMessageHandler>();
            services.TryAddTransient<DiscoveryRandomHttpMessageHandler>();
            services.TryAddTransient<DiscoveryRoundRobinHttpMessageHandler>();

            return services;
        }

        protected IConfiguration Configuration { get; set; }
    }
}
