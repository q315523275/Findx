using System;
using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    public interface IChannelAccessor : IDisposable
    {
        IModel Channel { get; }

        string Name { get; }
    }
}