using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.Jobs;
using Findx.Pdf;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Findx.WebHost.Controllers;

/// <summary>
///     通用控制器
/// </summary>
[Description("通用")]
public class CommonController : ApiControllerBase
{
    private static readonly string[] Summaries =
        { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

    /// <summary>
    ///     雪花ID，Json返回需转换为string
    /// </summary>
    /// <returns></returns>
    [HttpGet("/snowflakeId")]
    public string SnowflakeId()
    {
        return SnowflakeIdUtility.Default().NextId().ToString();
    }

    /// <summary>
    ///     统一社会信用代码验证
    /// </summary>
    /// <param name="creditCode"></param>
    /// <returns></returns>
    [HttpGet("/verifyCreditCode")]
    public string VerifyCreditCode([Required] string creditCode)
    {
        return $"{CreditCodeUtility.IsCreditCode(creditCode)}|{CreditCodeUtility.RandomCreditCode()}";
    }


    /// <summary>
    ///     文本转PDF示例接口
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
    ///     Log4Net日志示例接口
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
    ///     异常
    /// </summary>
    /// <returns></returns>
    [HttpGet("/exception")]
    public async Task<string> Exception([FromServices] ILogger<CommonController> logger)
    {
        await Task.Delay(50);
        //return "1";
        var exp = new Exception("自定义异常");

        logger.LogError(exp, string.Empty);

        throw exp;
    }

    /// <summary>
    ///     异常
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
    ///     查询调度任务列表
    /// </summary>
    /// <param name="storage"></param>
    /// <returns></returns>
    [HttpGet("/jobs")]
    public async Task<object> QueryJobs([FromServices] IJobStorage storage)
    {
        return await storage.GetJobsAsync();
    }

    /// <summary>
    ///     防重复请求
    /// </summary>
    /// <returns></returns>
    [HttpGet("/antiDuplicateRequest")]
    [AntiDuplicateRequest(Interval = "10s", Type = LockType.Ip)]
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
    ///     公网访问限定
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
    ///     请求速率限定
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

    /// <summary>
    ///     命令执行
    /// </summary>
    /// <param name="command"></param>
    /// <param name="arguments"></param>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    [HttpGet("/cmd")]
    public async Task<string> Cmd([Required] string command, string arguments, string workingDirectory)
    {
        return await ProcessX.StartAsync(command, workingDirectory: workingDirectory).FirstAsync();
    }

    /// <summary>
    ///     进程销毁
    /// </summary>
    /// <returns></returns>
    [HttpGet("/kill")]
    public object Kill()
    {
        RuntimeUtility.Destroy();
        return DateTime.Now;
    }

    /// <summary>
    ///     将组合单词的字符串转换为横线连接的字符串
    /// </summary>
    /// <param name="ors"></param>
    /// <returns></returns>
    [HttpGet("/ToSnakeCase")]
    public string ToSnakeCase([Required] string ors)
    {
        return ors.ToSnakeCase();
    }


    /// <summary>
    ///     网络Ping
    /// </summary>
    /// <param name="ors"></param>
    /// <returns></returns>
    [HttpGet("/Ping")]
    public bool Ping([Required] string ip)
    {
        return NetUtility.Ping(ip);
    }
}