using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Email;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Findx.Messaging;
using Findx.WebHost.Messaging;
using Findx.Pdf;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;

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
        public async Task<CommonResult> LoadBalancer([FromServices] IDiscoveryClient client, [FromServices] ILoadBalancerProvider loadBalancer, [Required] string serviceName)
        {
            var all = await client.GetAllInstancesAsync();
            var list = await client.GetInstancesAsync(serviceName);
            var balancer = await loadBalancer.GetAsync(serviceName);
            var instance = await balancer.ResolveServiceInstanceAsync();

            return CommonResult.Success(new { AllInstances = all, Instances = list, Instance = instance, LoadBalancer = balancer.Name.ToString() });
        }
    }
}
