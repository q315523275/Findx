using System.Collections.Generic;

namespace Findx.RabbitMQ;

public class ExchangeDeclareConfiguration
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="exchangeName"></param>
    /// <param name="type"></param>
    /// <param name="durable"></param>
    /// <param name="autoDelete"></param>
    public ExchangeDeclareConfiguration(string exchangeName, string type, bool durable = false, bool autoDelete = false)
    {
        ExchangeName = exchangeName;
        Type = type;
        Durable = durable;
        AutoDelete = autoDelete;
        Arguments = new Dictionary<string, object>();
    }

    /// <summary>
    ///     交换机名称
    /// </summary>
    public string ExchangeName { get; }

    /// <summary>
    ///     交换机类型，常见的如fanout、direct、topic
    /// </summary>
    public string Type { get; }

    /// <summary>
    ///     设置是否持久化。durable设置true表示持久化，反之是持久化。
    ///     持久化可以将交换机存盘，在服务器重启时不会丢失相关信息
    /// </summary>
    public bool Durable { get; set; }

    /// <summary>
    ///     设置是否自动删除。autoDelete设置为true则表示自动删除。
    ///     自动删除的前提是至少有一个队列或者交换机与这个交换器绑定的队列或者交换器都与之解绑
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    ///     其他一些结构化参数
    ///     比如alternate-exchange
    /// </summary>
    public IDictionary<string, object> Arguments { get; set; }
}