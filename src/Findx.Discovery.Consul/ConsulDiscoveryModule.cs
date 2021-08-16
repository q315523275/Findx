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
    [Description("Findx-Consul服务发现模块")]
    public class ConsulDiscoveryModule : DiscoveryModuleBase
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 30;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            var section = Configuration.GetSection("Findx:Consul");
            services.Configure<ConsulOptions>(section);

            services.TryAddSingleton<IConsulServiceRegistry, ConsulServiceRegistry>();
            services.TryAddSingleton<IConsulRegistration, ConsulRegistration>();
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(IServiceInstanceProvider), typeof(ConsulServiceInstanceProvider), ServiceLifetime.Singleton);
            services.Replace(descriptor);
            services.TryAddSingleton<IConsulClient>(sp => { return ConsulClientFactory.CreateClient(sp.GetRequiredService<IOptionsMonitor<ConsulOptions>>().CurrentValue); });

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            Task.Run(() =>
            {
                IOptionsMonitor<DiscoveryOptions> _optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
                if (_optionsMonitor.CurrentValue.Enabled && _optionsMonitor.CurrentValue.Register)
                {
                    IConsulServiceRegistry consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
                    IConsulRegistration consulRegistration = provider.GetRequiredService<IConsulRegistration>();
                    consulServiceRegistry.Register(consulRegistration).ConfigureAwait(false).GetAwaiter();
                }
            });
            base.UseModule(provider);
        }

        public override void OnShutdown(IServiceProvider provider)
        {
            IOptionsMonitor<DiscoveryOptions> _optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
            if (_optionsMonitor.CurrentValue.Enabled && _optionsMonitor.CurrentValue.Deregister)
            {
                IConsulServiceRegistry consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
                IConsulRegistration consulRegistration = provider.GetRequiredService<IConsulRegistration>();
                consulServiceRegistry.Deregister(consulRegistration).ConfigureAwait(false).GetAwaiter();
            }
            base.OnShutdown(provider);
        }
    }
}
