using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    [Description("Findx-RabbitMQ模块")]
    public class FindxRabbitMqModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;

        public override int Order => 50;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            services.Configure<FindxRabbitMqOptions>(configuration.GetSection("Findx:RabbitMQ"));

            // MQ连接池
            services.AddSingleton<IConnectionPool, ConnectionPool>();
            services.AddSingleton<IChannelPool, ChannelPool>();

            // MQ消费工厂
            services.AddSingleton<IRabbitMqConsumerFactory, RabbitMqConsumerFactory>();
            services.AddTransient<RabbitMqConsumer>();
            // MQ序列化,使用json
            services.AddSingleton<IRabbitMqSerializer, DefaultRabbitMqSerializer>();
            // MQ推送消息
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            // 注解消费者
            services.AddSingleton<IRabbitConsumerBuilder, RabbitConsumerBuilder>();
            services.GetOrAddTypeFinder<IRabbitConsumerFinder>(assemblyFinder => new RabbitConsumerFinder(assemblyFinder));

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            Task.Run(() => { provider.GetService<IRabbitConsumerBuilder>()?.Build(); });
            // provider.GetService<IRabbitConsumerBuilder>()?.Build();
            base.UseModule(provider);
        }

    }
}
