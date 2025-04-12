using Microsoft.Extensions.Options;

namespace Findx.RabbitMQ;

/// <summary>
///     Options配置
/// </summary>
public class FindxRabbitMqOptions: IOptions<FindxRabbitMqOptions>
{
    /// <summary>
    ///     连接信息集合对象
    /// </summary>
    public RabbitMqConnections Connections { get; set; } = new();

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    ///     Value
    /// </summary>
    public FindxRabbitMqOptions Value => this;
}