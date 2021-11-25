using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.RabbitMQ;
using Findx.WebHost.EventBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Findx.WebHost.RabbitMQ
{
    [Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
    [RabbitListener]
    public class DefaultConsumer
    {
        [RabbitConsumer("findx_event_bus", "direct", "Findx.Consumer0", 1, "Findx.WebHost.EventBus.FindxTestEvent")]
        public async Task<string> ReceiveMessage(FindxTestEvent time)
        {
            throw new System.Exception("自定义异常");

            Console.WriteLine("message time is:" + time.ToJson());
            await Task.Delay(3);
            return "1";
        }
    }
}
