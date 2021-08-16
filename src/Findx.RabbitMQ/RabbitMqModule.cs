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
    public class RabbitMQModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;

        public override int Order => 50;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:RabbitMQ");
            services.Configure<RabbitMQOptions>(section);

            // MQ连接池
            services.AddSingleton<IConnectionPool, ConnectionPool>();
            // MQ消费工厂
            services.AddSingleton<IRabbitMQConsumerFactory, RabbitMQConsumerFactory>();
            // MQ序列化,使用json
            services.AddSingleton<IRabbitMQSerializer, DefaultRabbitMqSerializer>();
            // MQ推送消息
            services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

            // 注解消费者
            services.AddSingleton<IRabbitConsumerBuilder, RabbitConsumerBuilder>();
            services.GetOrAddTypeFinder<IRabbitConsumerFinder>(assemblyFinder => new RabbitConsumerFinder(assemblyFinder));
            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            Task.Run(() => { provider.GetService<IRabbitConsumerBuilder>()?.Build(); });
            base.UseModule(provider);
        }

    }
}
