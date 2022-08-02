using Consul;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Findx.Discovery.Consul
{
    /// <summary>
    /// Findx-Consul服务发现模块
    /// </summary>
    [Description("Findx-Consul服务发现模块")]
    public class ConsulDiscoveryModule : DiscoveryModuleBase
    {
        /// <summary>
        /// 模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        /// 模块排序
        /// </summary>
        public override int Order => 30;

        /// <summary>
        /// 模块配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            var section = Configuration.GetSection("Findx:Consul");
            services.Configure<ConsulOptions>(section);

            services.TryAddSingleton<IConsulServiceRegistry, ConsulServiceRegistry>();
            services.TryAddSingleton<IConsulRegistration, ConsulRegistration>();
            var descriptor = new ServiceDescriptor(typeof(IServiceInstanceProvider), typeof(ConsulServiceInstanceProvider), ServiceLifetime.Singleton);
            services.Replace(descriptor);
            services.TryAddSingleton<IConsulClient>(sp => ConsulClientFactory.CreateClient(sp.GetRequiredService<IOptionsMonitor<ConsulOptions>>().CurrentValue));

            return services;
        }

        /// <summary>
        /// 模块启用
        /// </summary>
        /// <param name="provider"></param>
        public override void UseModule(IServiceProvider provider)
        {
            Task.Run(() =>
            {
                var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
                if (!optionsMonitor.CurrentValue.Enabled || !optionsMonitor.CurrentValue.Register) return;
                var consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
                var consulRegistration = provider.GetRequiredService<IConsulRegistration>();
                consulServiceRegistry.Register(consulRegistration).ConfigureAwait(false).GetAwaiter();
            });
            base.UseModule(provider);
        }

        /// <summary>
        /// 模块销毁
        /// </summary>
        /// <param name="provider"></param>
        public override void OnShutdown(IServiceProvider provider)
        {
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
            if (optionsMonitor.CurrentValue.Enabled && optionsMonitor.CurrentValue.Deregister)
            {
                var consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
                var consulRegistration = provider.GetRequiredService<IConsulRegistration>();
                consulServiceRegistry.Deregister(consulRegistration).ConfigureAwait(false).GetAwaiter();
            }
            base.OnShutdown(provider);
        }
    }
}
