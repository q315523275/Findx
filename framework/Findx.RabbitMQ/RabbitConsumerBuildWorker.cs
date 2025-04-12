using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Findx.RabbitMQ;

/// <summary>
///     消费者构建工作者
/// </summary>
public class RabbitConsumerBuildWorker : BackgroundService
{
    private readonly IRabbitConsumerBuilder _builder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="builder"></param>
    public RabbitConsumerBuildWorker(IRabbitConsumerBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    ///    构建执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _builder.BuildAsync(stoppingToken);
    }
}