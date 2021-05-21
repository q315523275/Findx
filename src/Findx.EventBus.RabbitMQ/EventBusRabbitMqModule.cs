using Findx.EventBus.Abstractions;
using Findx.Extensions;
using Findx.Modularity;
using Findx.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Findx.EventBus.RabbitMQ
{
    [Description("Findx-消息总线模块")]
    [DependsOnModules(typeof(RabbitMqModule))]
    public class EventBusRabbitMqModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 30;
        /// <summary>
        /// 配置服务注册
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:EventBus:RabbitMQ");
            services.Configure<EventBusRabbitMqOptions>(section);

            services.AddSingleton<IEventBus, EventBusRabbitMQ>();
            services.AddSingleton<IEventPublisher, EventRabbitMqPublisher>();
            services.AddSingleton<IEventSubscriber, EventRabbitMqSubscriber>();

            return services;
        }
    }
}
