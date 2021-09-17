using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Discovery;
using Findx.Drawing;
using Findx.Email;
using Findx.EventBus;
using Findx.Extensions;
using Findx.Messaging;
using Findx.Pdf;
using Findx.RabbitMQ;
using Findx.Security.Authorization;
using Findx.Tasks.Scheduling;
using Findx.WebHost.EventBus;
using Findx.WebHost.Messaging;
using Findx.WebHost.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
        public JsonResult Health()
        {
            return new JsonResult(new { status = "UP" });
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
        /// 雪花ID
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpGet("/snowflakeId")]
        public long SnowflakeId()
        {
            return Findx.Utils.SnowflakeId.Default().NextId();
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

        /// <summary>
        /// 异常
        /// </summary>
        /// <returns></returns>
        [HttpGet("/exception")]
        public string Exception()
        {
            throw new Exception("自定义异常");
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <returns></returns>
        [HttpGet("/exception_timeout")]
        public async Task<string> Exception_Timeout()
        {
            await Task.Delay(3000);
            return "1";
        }

        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApiClient_Discovery")]
        public async Task<string> WebApiClientDiscovery([FromServices] IFindxApi api)
        {
            return await api.ApplicationInfo();
        }

        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApiClient_Discovery_Exception")]
        public async Task<string> WebApiClientDiscoveryException([FromServices] IFindxApi api)
        {
            return await api.Exception();
        }

        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApiClient_Discovery_Timeout")]
        public async Task<string> WebApiClientDiscoveryTimeout([FromServices] IFindxApi api)
        {
            return await api.Timeout();
        }

        /// <summary>
        /// 查询调度任务列表
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        [HttpGet("/jobs")]
        public async Task<object> QueryJobs([FromServices] IScheduledTaskStore storage)
        {
            return await storage.GetTasksAsync();
        }

        /// <summary>
        /// 防重复请求
        /// </summary>
        /// <returns></returns>
        [HttpGet("/antiDuplicateRequest")]
        [AntiDuplicateRequest]
        public object AntiDuplicateRequest()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// 公网访问限定
        /// </summary>
        /// <returns></returns>
        [HttpGet("/privateNetworkLimiter")]
        [PrivateNetworkLimiter]
        public object PrivateNetworkLimiter()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// 请求速率限定
        /// </summary>
        /// <returns></returns>
        [HttpGet("/rateLimit")]
        [RateLimiter(Period = "10s", Limit = 10)]
        public object RateLimiter()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/imageSharp_makeThumbnail")]
        public IActionResult ImageSharp([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, int width, int height, ImageResizeMode mode = ImageResizeMode.Crop)
        {
            var img = imageProcessor.MakeThumbnail(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", width, height, mode);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/imageSharp_imageWatermark")]
        public IActionResult ImageWatermark([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, string filePath2, int location, float opacity)
        {
            var img = imageProcessor.ImageWatermark(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", applicationInstance.MapPath(filePath2), location, opacity);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/imageSharp_letterWatermark")]
        public IActionResult LetterWatermark([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, string text, int location, int fontSize, string fontPath)
        {
            var img = imageProcessor.LetterWatermark(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", text, location, fontPath, fontSize);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图片验证码
        /// </summary>
        /// <param name="verifyCoder"></param>
        /// <returns></returns>
        [HttpGet("/verifyCode")]
        public async Task<IActionResult> VerifyCode([FromServices] IVerifyCoder verifyCoder, int width = 80, int height = 35)
        {
            var code = verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);

            var img = await verifyCoder.CreateImageAsync(code, width: width, height: height);

            return File(img, "image/jpeg");
        }
    }
}
