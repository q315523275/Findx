﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.Jobs.Storage;
using Findx.Pdf;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Findx.WebHost.Controllers;

/// <summary>
///     通用控制器
/// </summary>
[Route("api/common")]
[Description("公共服务"), Tags("公共服务")]
public class CommonController : ApiControllerBase
{
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    /// <summary>
    ///     雪花ID，Json返回需转换为string
    /// </summary>
    /// <returns></returns>
    [HttpGet("snowflakeId")]
    public long SnowflakeId()
    {
        return SnowflakeIdUtility.Default().NextId();
    }

    /// <summary>
    ///     统一社会信用代码验证
    /// </summary>
    /// <param name="creditCode"></param>
    /// <returns></returns>
    [HttpGet("verifyCreditCode")]
    public string VerifyCreditCode([Required] string creditCode)
    {
        return $"{CreditCodeUtility.IsCreditCode(creditCode)}|{CreditCodeUtility.RandomCreditCode()}";
    }

    /// <summary>
    ///     文本转PDF示例接口
    /// </summary>
    /// <param name="converter"></param>
    /// <param name="text"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("textToPdf")]
    public async Task<IActionResult> TextToPdfAsync([FromServices] IPdfConverter converter, [Required] string text, CancellationToken cancellationToken)
    {
        var res = await converter.ConvertAsync(text, cancellationToken);
        return File(res, "application/pdf");
    }

    /// <summary>
    ///     日志组件示例接口
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    [HttpGet("log")]
    public CommonResult Log([FromServices] ILogger<CommonController> logger)
    {
        logger.LogInformation("这是一条日志组件正常日志信息");
        logger.LogError("这是一条日志组件异常日志信息");
        return CommonResult.Success();
    }

    /// <summary>
    ///     异常
    /// </summary>
    /// <returns></returns>
    [HttpGet("exception")]
    public string Exception([FromServices] ILogger<CommonController> logger)
    {
        // await Task.Delay(50);
        // return "1";
        var exp = new Exception("自定义异常");
        // logger.LogError(exp, string.Empty);
        throw exp;
    }

    /// <summary>
    ///     超时
    /// </summary>
    /// <returns></returns>
    [HttpGet("timeout")]
    public async Task<string> TimeoutAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now} - 耗时接口");
        await Task.Delay(3000, cancellationToken);
        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     查询调度任务列表
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("jobs")]
    public async Task<object> JobsAsync([FromServices] IJobStorage storage, CancellationToken cancellationToken)
    {
        return await storage.GetJobsAsync(cancellationToken);
    }

    /// <summary>
    ///     防重复请求
    /// </summary>
    /// <returns></returns>
    [HttpGet("antiDuplicateRequest")]
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
    [HttpGet("privateNetworkLimiter")]
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
    [HttpGet("rateLimit")]
    [RateLimiter(Period = "10s", Limit = 5)]
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("cmd")]
    public async Task<object> CmdAsync([Required] string command, CancellationToken cancellationToken)
    {
        return await ProcessX.StartAsync(command).ToTask(cancellationToken);
    }

    /// <summary>
    ///     进程销毁
    /// </summary>
    /// <returns></returns>
    [HttpGet("kill")]
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
    [HttpGet("toSnakeCase")]
    public string ToSnakeCase([Required] string ors)
    {
        return ors.ToSnakeCase();
    }
    
    /// <summary>
    ///     网络Ping
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    [HttpGet("ping")]
    public bool Ping([Required] string ip)
    {
        return NetUtility.Ping(ip);
    }
}