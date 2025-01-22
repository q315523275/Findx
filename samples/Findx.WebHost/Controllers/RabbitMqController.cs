using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     RabbitMQ
/// </summary>
[Route("api/rabbitMq")]
[Description("事件总线"), Tags("事件总线")]
public class RabbitMqController : ApiControllerBase
{
    // /// <summary>
    // ///     RabbitMQ消息推送示例接口
    // /// </summary>
    // /// <param name="publisher"></param>
    // /// <param name="message"></param>
    // /// <param name="exchangeName"></param>
    // /// <param name="exchangeType"></param>
    // /// <param name="routingKey"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [HttpGet("publish")]
    // public CommonResult PublishAsync([FromServices] IRabbitMqPublisher publisher, [Required] string message, [Required] string exchangeName, [Required] string exchangeType, [Required] string routingKey, CancellationToken cancellationToken)
    // {
    //     publisher.Publish(message, exchangeName, exchangeType, routingKey);
    //     return CommonResult.Success();
    // }
}