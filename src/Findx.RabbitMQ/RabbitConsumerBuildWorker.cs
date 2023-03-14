using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Findx.RabbitMQ
{
    /// <summary>
    /// 消费者构建工作者
    /// </summary>
    public class RabbitConsumerBuildWorker: BackgroundService
    {
        private readonly IRabbitConsumerBuilder _builder;

        public RabbitConsumerBuildWorker(IRabbitConsumerBuilder builder)
        {
            _builder = builder;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _builder.Build();

            return Task.CompletedTask;
        }
    }
}