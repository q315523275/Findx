﻿using System.ComponentModel;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Messaging;
using Findx.Utilities;
using Findx.WebHost.EventHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     进程消息服务
/// </summary>
[Route("api/message")]
[Description("进程消息服务"), Tags("进程消息服务")]
public class MessageController : ApiControllerBase
{
    /// <summary>
    ///     消息发送(CQRS)示例接口
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    [HttpGet("request")]
    public async Task<string> MessageSend([FromServices] IMessageDispatcher dispatcher)
    {
        var orderId = SnowflakeIdUtility.Default().NextId();
        var res = await dispatcher.SendAsync(new CancelOrderCommand(orderId.ToString()));
        return $"orderId:{orderId},result:{res}";
    }

    /// <summary>
    ///     消息通知示例接口
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [HttpGet("notify")]
    public async Task<string> MessageNotify([FromServices] IMessageDispatcher context)
    {
        await context.PublishAsync(new PayedOrderCommand(SnowflakeIdUtility.Default().NextId()));
        return "ok";
    }
}