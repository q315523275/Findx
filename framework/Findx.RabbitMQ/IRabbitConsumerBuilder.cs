using System.Threading;
using System.Threading.Tasks;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ消费者构建器
/// </summary>
public interface IRabbitConsumerBuilder
{
    /// <summary>
    ///     构建消费者
    /// </summary>
    Task BuildAsync(CancellationToken cancellationToken = default);
}