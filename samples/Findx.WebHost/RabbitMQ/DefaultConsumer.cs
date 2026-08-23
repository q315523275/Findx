using System;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.WebHost.RabbitMQ;

/// <summary>
///     
/// </summary>
[Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
[RabbitListener]
public class DefaultConsumer
{
    /// <summary>
    ///     RabbitConsumer
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    [RabbitConsumer("findx_event_bus", "direct", "Findx.Consumer", 1, "Findx.WebHost.EventBus.FindxTestEvent")]
    public async Task<string> ReceiveMessage(FindxTestEvent time)
    {
        Console.WriteLine("Findx.WebHost.EventBus.FindxTestEvent message time is:" + time.ToJson() + "-----" + DateTime.Now.ToLongTimeString());
        await Task.Delay(3);
        return "1";
    }
    
    /// <summary>
    ///     RabbitConsumer
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [RabbitConsumer("findx_event_bus", "direct", "Findx.Consumer", 1, "Findx.WebHost.RabbitConsumer")]
    public async Task<string> ReceiveMessage(string message)
    {
        await Task.Delay(3);
        Console.WriteLine("Findx.WebHost.RabbitConsumer message is:" + message + "-----" + DateTime.Now.ToLongTimeString());
        return "222222";
    }
}