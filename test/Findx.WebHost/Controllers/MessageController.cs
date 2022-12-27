﻿using Findx.Messaging;
using Findx.WebHost.EventHandlers;
using Findx.Extensions;
using Findx;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class MessageController : Controller
    {
        /// <summary>
        /// 消息发送(CQRS)示例接口
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        [HttpGet("/message/request")]
        public async Task<string> MessageSend([FromServices] IMessageDispatcher dispatcher)
        {
            var orderId = Findx.Utils.SnowflakeId.Default().NextId();
            var res = await dispatcher.SendAsync(new CancelOrderCommand(orderId.ToString()));
            return $"orderId:{orderId},result:{res}";
        }

        /// <summary>
        /// 消息通知示例接口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpGet("/message/notify")]
        public async Task<string> MessageNotify([FromServices] IApplicationContext context)
        {
            await context.PublishEventAsync(new PayedOrderCommand(0));
            return "ok";
        }
    }
}