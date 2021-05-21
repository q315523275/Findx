using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    public interface IRabbitMqConsumer
    {
        void Bind(string routingKey);

        void Unbind(string routingKey);

        void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback);

        void StartConsuming();
    }
}
