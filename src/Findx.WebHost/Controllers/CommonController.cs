using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Findx.Email;
using Findx.EventBus.Abstractions;
using Findx.Messaging;
using Findx.Pdf;
using Findx.RabbitMQ;
using Findx.Security.Authorization;
using Findx.WebHost.EventBus;
using Findx.WebHost.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    /// <summary>
    /// 通用控制器
    /// </summary>
    public class CommonController : ApiControllerBase
    {
        /// <summary>
        /// 健康检查地址
        /// </summary>
        /// <returns></returns>
        [HttpGet("/health")]
        public string Health()
        {
            return DateTime.Now.ToString();
        }

        /// <summary>
        /// 应用基础信息
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpGet("/applicationInfo")]
        public CommonResult ApplicationInfo([FromServices] IApplicationInstanceInfo instance)
        {
            return CommonResult.Success(instance);
        }

        /// <summary>
        /// 权限数据查询示例接口
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("/permission")]
        // [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
        public async Task<CommonResult> Permission([FromServices] IPermissionStore store)
        {
            var res = await store.GetFromStoreAsync();
            return CommonResult.Success(res);
        }

        /// <summary>
        /// 限速示例接口
        /// </summary>
        /// <returns></returns>
        [HttpGet("/rateLimit")]
        [RateLimiter(Limit = 30)]
        public string RateLimit()
        {
            return DateTime.Now.ToString();
        }

        /// <summary>
        /// 邮件发送示例接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpGet("/emailSend")]
        public async Task<string> EmailSend([FromServices] IEmailSender sender, [Required] string mailAddress, [Required] string subject, [Required] string body)
        {
            await sender.SendAsync(mailAddress, subject, body, isBodyHtml: false);

            return "ok";
        }

        /// <summary>
        /// 消息发送(CQRS)示例接口
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        [HttpGet("/messageSend")]
        public async Task<string> MessageSend([FromServices] IMessageSender sender)
        {
            var orderId = Findx.Utils.SnowflakeId.Default().NextId();
            var res = await sender.SendAsync(new CancelOrderCommand(orderId));
            return $"orderId:{orderId},result:{res}";
        }

        /// <summary>
        /// 消息通知示例接口
        /// </summary>
        /// <param name="notifySender"></param>
        /// <returns></returns>
        [HttpGet("/messageNotify")]
        public async Task<string> MessageNotify([FromServices] IMessageNotifySender notifySender)
        {
            await notifySender.PublishAsync(new PayedOrderCommand(0));
            return "ok";
        }

        /// <summary>
        /// 文本转PDF示例接口
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet("/textToPdf")]
        public async Task<IActionResult> TextToPdf([FromServices] IPdfConverter converter, [Required] string text)
        {
            var res = await converter.ConvertAsync(text);
            return File(res, "application/pdf");
        }

        /// <summary>
        /// 服务发现查询示例接口
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [HttpGet("/discovery")]
        public async Task<CommonResult> Discovery([FromServices] IDiscoveryClient client, [Required] string serviceName)
        {
            var all = await client.GetAllInstancesAsync();
            var list = await client.GetInstancesAsync(serviceName);
            return CommonResult.Success(new { AllInstances = all, Instances = list });
        }

        /// <summary>
        /// 服务发现+负载查询示例接口
        /// </summary>
        /// <param name="client"></param>
        /// <param name="loadBalancer"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [HttpGet("/loadBalancer")]
        public async Task<CommonResult> LoadBalancer([FromServices] IDiscoveryClient client, [FromServices] ILoadBalancerProvider loadBalancer, [Required] string serviceName, string loadBalancerType = "Random")
        {
            var balancer = await loadBalancer.GetAsync(serviceName, loadBalancerType.To<LoadBalancerType>());
            var instance = await balancer.ResolveServiceInstanceAsync();

            return CommonResult.Success(new { Instance = instance, LoadBalancer = balancer.Name.ToString() });
        }

        /// <summary>
        /// Log4Net日志示例接口
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        [HttpGet("/log4Net")]
        public CommonResult Log4Net([FromServices] ILogger<CommonController> logger)
        {
            logger.LogInformation($"这是一条Log4Net正常日志信息{0}", DateTime.Now);
            logger.LogError($"这是一条Log4Net异常日志信息{0}", DateTime.Now);

            return CommonResult.Success();
        }

        /// <summary>
        /// RabbitMQ消息推送示例接口
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="exchangeType"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        [HttpGet("/rabbitPublish")]
        public CommonResult RabbitPublish([FromServices] IRabbitMQPublisher publisher, [Required] string message, [Required] string exchangeName, [Required] string exchangeType, [Required] string routingKey)
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
        [HttpGet("/eventBusPublish")]
        public CommonResult EventBusPublish([FromServices] IEventPublisher publisher, [Required] string message)
        {
            var eventInfo = new FindxTestEvent { Body = message };
            publisher.Publish(eventInfo);
            return CommonResult.Success(eventInfo);
        }

    }
}
