using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Findx.RabbitMQ
{
    [Description("Findx-RabbitMQ模块")]
    public class RabbitMqModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 20;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:RabbitMQ");
            services.Configure<RabbitMQOptions>(section);

            // MQ连接池
            services.AddSingleton<IConnectionPool, ConnectionPool>();
            // MQ消费工厂
            services.AddSingleton<IRabbitMqConsumerFactory, RabbitMqConsumerFactory>();
            // MQ序列化,使用json
            services.AddSingleton<IRabbitMqSerializer, DefaultRabbitMqSerializer>();
            // MQ推送消息
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            return services;
        }

    }
}
