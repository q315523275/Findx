using Findx.Data;
using Findx.ExceptionHandling;
using Findx.Exceptions;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc.Middlewares
{
    /// <summary>
    /// 异常JSON处理中间件
    /// </summary>
    public class JsonExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonExceptionHandlerMiddleware> _logger;
        private readonly IExceptionNotifier _exceptionNotifier;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="exceptionNotifier"></param>
        public JsonExceptionHandlerMiddleware(RequestDelegate next, ILogger<JsonExceptionHandlerMiddleware> logger, IExceptionNotifier exceptionNotifier)
        {
            _next = next;
            _logger = logger;
            _exceptionNotifier = exceptionNotifier;
        }



        /// <summary>
        /// 执行中间件拦截逻辑
        /// </summary>
        /// <param name="context">Http上下文</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var js = DateTime.Now;
            try
            {
                await _next(context);
            }
            catch (FindxException ex)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json;charset=utf-8";
                await context.Response.WriteAsync(CommonResult.Fail(ex.ErrorCode ?? "500", ex.Message ?? "未知异常,请稍后再试").ToJson());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"全局异常捕获,状态码:{ context?.Response?.StatusCode},Url:{context?.Request?.GetDisplayUrl()}");
                await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));

                // 开启异常,方便外层组件熔断等功能使用
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain;charset=utf-8";
                await context.Response.WriteAsync("error occurred");
            }
            Console.WriteLine($"第一中间件接口耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");
        }
    }
}
