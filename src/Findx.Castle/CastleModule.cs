using System.ComponentModel;
using Castle.DynamicProxy;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Castle
{
    [Description("Findx-Castle代理模块")]
	public class CastleModule : FindxModule
	{
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Application;

        /// <summary>
        /// 排序
        /// </summary>
        public override int Order => 50;
        
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(CastleAsyncDeterminationInterceptor<>));
            services.AddSingleton<IProxyGenerator, ProxyGenerator>();
            return services;
        }
    }
}

