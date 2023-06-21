using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Findx.Data;
using Findx.ExceptionHandling;
using Findx.Exceptions;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Findx.AspNetCore.Mvc.Middlewares;

/// <summary>
///     异常JSON处理中间件
/// </summary>
public class JsonExceptionHandlerMiddleware
{
    private readonly IExceptionNotifier _exceptionNotifier;
    private readonly ILogger<JsonExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionNotifier"></param>
    public JsonExceptionHandlerMiddleware(RequestDelegate next, ILogger<JsonExceptionHandlerMiddleware> logger,
        IExceptionNotifier exceptionNotifier)
    {
        _next = next;
        _logger = logger;
        _exceptionNotifier = exceptionNotifier;
    }

    /// <summary>
    ///     执行中间件拦截逻辑
    /// </summary>
    /// <param name="context">Http上下文</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FindxException ex)
        {
            await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            await WriteOkFailAsync(context, ex.ErrorCode ?? "500", ex.Message ?? "未知异常,请稍后再试");
        }
        catch (ValidationException ex)
        {
            await WriteOkFailAsync(context, "4001", ex.Message ?? "参数校验不通过");
        }
        catch (BrokenCircuitException ex)
        {
            await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            await WriteOkFailAsync(context, "500", "当前服务不可用,请稍后再试");
        }
        catch (TimeoutRejectedException ex)
        {
            await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            await WriteOkFailAsync(context, "500", "内部服务超时,请稍后再试");
        }
        catch (HttpRequestException ex)
        {
            await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            await WriteOkFailAsync(context, ex.StatusCode?.ToString() ?? "500", "上游系统调用异常");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "全局异常捕获,Url:{DisplayUrl}", context?.Request?.GetDisplayUrl());

            await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));

            if (context == null || context.Response.HasStarted) 
                return;
            // 开启异常,方便外层组件熔断等功能使用
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/plain;charset=utf-8";
            await context.Response.WriteAsync("error occurred");
        }
    }

    /// <summary>
    /// write body
    /// </summary>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="message"></param>
    private static async Task WriteOkFailAsync(HttpContext context, string code, string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json;charset=utf-8"; ;
        await context.Response.WriteAsync("{\"code\":\"" + code + "\",\"msg\":\"" + message + "\"}");
    }
}