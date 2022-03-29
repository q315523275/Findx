using Findx.Data;
using Findx.EventBus;
using Findx.Extensions;
using Findx.RabbitMQ;
using Findx.WebHost.EventBus;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Findx.WebHost.Controllers
{
    public class EventBusController : Controller
    {
        /// <summary>
        /// RabbitMQ消息推送示例接口
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="exchangeType"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        [HttpGet("/rabbit/publish")]
        public CommonResult RabbitPublish([FromServices] IRabbitMqPublisher publisher, [Required] string message, [Required] string exchangeName, [Required] string exchangeType, [Required] string routingKey)
        {
            publisher.Publish(message, exchangeName, exchangeType, routingKey);
            return CommonResult.Success();
        }

        /// <summary>
        /// EventBus事件推送示例接口
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet("/eventBus/publish")]
        public CommonResult EventBusPublish([FromServices] IEventPublisher publisher, [Required] string message)
        {
            var eventInfo = new FindxTestEvent { Body = message };
            Console.WriteLine($"push mesg:{eventInfo.ToJson()}");
            publisher.Publish(eventInfo);
            return CommonResult.Success(eventInfo);
        }
    }
}
