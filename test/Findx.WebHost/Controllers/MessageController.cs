using System.Threading.Tasks;
using Findx.Events;
using Findx.Messaging;
using Findx.Utils;
using Findx.WebHost.EventHandlers;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

public class MessageController : Controller
{
    /// <summary>
    ///     消息发送(CQRS)示例接口
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    [HttpGet("/message/request")]
    public async Task<string> MessageSend([FromServices] IMessageDispatcher dispatcher)
    {
        var orderId = SnowflakeId.Default().NextId();
        var res = await dispatcher.SendAsync(new CancelOrderCommand(orderId.ToString()));
        return $"orderId:{orderId},result:{res}";
    }

    /// <summary>
    ///     消息通知示例接口
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [HttpGet("/message/notify")]
    public async Task<string> MessageNotify([FromServices] IMessageDispatcher context)
    {
        await context.PublishAsync(new PayedOrderCommand(SnowflakeId.Default().NextId()));
        return "ok";
    }
    
    /// <summary>
    ///     消息通知示例接口
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [HttpGet("/message/bus")]
    public async Task<string> Bus([FromServices] IEventBus context)
    {
        var command = new PayedOrderCommand(SnowflakeId.Default().NextId());
        var orderId = command.OrderId;
        await context.PublishAsync(command);
        return $"IEventBus.PublishAsync###{orderId}****HandleAsync{command.OrderId}";
    }
}