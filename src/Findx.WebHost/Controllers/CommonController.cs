using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.Pdf;
using Findx.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
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
        /// 雪花ID，Json返回需转换为string
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpGet("/snowflakeId")]
        public long SnowflakeId()
        {
            return Findx.Utils.SnowflakeId.Default().NextId();
        }

        /// <summary>
        /// 统一社会信用代码验证
        /// </summary>
        /// <param name="creditCode"></param>
        /// <returns></returns>
        [HttpGet("/verifyCreditCode")]
        public string VerifyCreditCode([Required] string creditCode)
        {
            return $"{Findx.Utils.CreditCode.IsCreditCode(creditCode)}|{Findx.Utils.CreditCode.RandomCreditCode()}";
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
        /// 异常
        /// </summary>
        /// <returns></returns>
        [HttpGet("/exception")]
        public async Task<string> Exception()
        {
            await Task.Delay(50);
            //return "1";
            Console.WriteLine($"{DateTime.Now} - 自定义异常");
            throw new Exception("自定义异常");
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <returns></returns>
        [HttpGet("/exception/timeout")]
        public async Task<string> Exception_Timeout()
        {
            Console.WriteLine($"{DateTime.Now} - 耗时接口");
            await Task.Delay(3000);
            return "1";
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
            });
        }

        /// <summary>
        /// 公网访问限定
        /// </summary>
        /// <returns></returns>
        [HttpGet("/privateNetworkLimiter")]
        [InternalNetworkLimiter]
        public object PrivateNetworkLimiter()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
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
            });
        }

        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        /// <summary>
        /// 命令执行
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        [HttpGet("/cmd")]
        public object Cmd([Required] string exePath, string arguments)
        {
            if (arguments.IsNullOrWhiteSpace())
                return Findx.Utils.RuntimeUtil.ExecForStringList(exePath);
            else
                return Findx.Utils.RuntimeUtil.ExecForStringList(exePath, arguments);
        }

        /// <summary>
        /// 进程销毁
        /// </summary>
        /// <returns></returns>
        [HttpGet("/kill")]
        public object Kill()
        {
            Findx.Utils.RuntimeUtil.Destroy();
            return DateTime.Now;
        }

        /// <summary>
        /// 将组合单词的字符串转换为横线连接的字符串
        /// </summary>
        /// <param name="ors"></param>
        /// <returns></returns>
        [HttpGet("/ToSnakeCase")]
        public string ToSnakeCase([Required] string ors)
        {
            return ors.ToSnakeCase();
        }


        /// <summary>
        /// 网络Ping
        /// </summary>
        /// <param name="ors"></param>
        /// <returns></returns>
        [HttpGet("/Ping")]
        public bool Ping([Required] string ip)
        {
            return Findx.Utils.NetUtil.Ping(ip);
        }
    }
}
