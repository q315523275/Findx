namespace Findx.RabbitMQ;

public interface IChannelPool
{
    IChannelAccessor Acquire(string channelName = null, string connectionName = null);
}