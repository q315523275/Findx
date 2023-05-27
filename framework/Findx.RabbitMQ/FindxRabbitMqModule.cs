using System.ComponentModel;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.RabbitMQ
{
    /// <summary>
    ///     Findx-RabbitMQ模块
    /// </summary>
    [Description("Findx-RabbitMQ模块")]
    public class FindxRabbitMqModule : FindxModule
    {
        /// <summary>
        ///     等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        ///     排序
        /// </summary>
        public override int Order => 110;

        /// <summary>
        ///     配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            var configuration = services.GetConfiguration();
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
            services.GetOrAddTypeFinder<IRabbitConsumerFinder>(assemblyFinder =>
                new RabbitConsumerFinder(assemblyFinder));

            // 消费者自动构建
            services.AddHostedService<RabbitConsumerBuildWorker>();

            return services;
        }
    }
}