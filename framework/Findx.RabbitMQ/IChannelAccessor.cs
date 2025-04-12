using System;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     虚拟连接访问器
/// </summary>
public interface IChannelAccessor : IDisposable
{
    /// <summary>
    ///     连接
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    ///     名称
    /// </summary>
    string Name { get; }
}