using System;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Findx.AspNetCore.Mvc.Middlewares;

/// <summary>
///     跟踪标识中间件
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly IOptions<CorrelationIdOptions> _options;
    
    private readonly ICorrelationIdProvider _correlationIdProvider;
    
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="correlationIdProvider"></param>
    /// <param name="next"></param>
    /// <param name="options"></param>
    public CorrelationIdMiddleware(ICorrelationIdProvider correlationIdProvider, RequestDelegate next, IOptions<CorrelationIdOptions> options)
    {
        _correlationIdProvider = correlationIdProvider;
        _next = next;
        _options = options;
    }
    
    /// <summary>
    /// 执行中间件拦截逻辑
    /// </summary>
    /// <param name="context">Http上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationIdFromRequest(context);
        using (_correlationIdProvider.Change(correlationId))
        {
            CheckAndSetCorrelationIdOnResponse(context, _options.Value, correlationId);
            await _next(context);
        }
    }

    /// <summary>
    ///     从请求中获取跟踪标识
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual string GetCorrelationIdFromRequest(HttpContext context)
    {
        var correlationId = context.Request.Headers[_options.Value.HttpHeaderName];
        if (correlationId.IsNullOrEmpty())
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[_options.Value.HttpHeaderName] = correlationId;
        }

        return correlationId;
    }

    /// <summary>
    ///     检查并设置跟踪关联ID在响应内容
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="options"></param>
    /// <param name="correlationId"></param>
    protected virtual void CheckAndSetCorrelationIdOnResponse(HttpContext httpContext, CorrelationIdOptions options, string correlationId)
    {
        httpContext.Response.OnStarting(() =>
        {
            if (options.SetResponseHeader && !httpContext.Response.Headers.ContainsKey(options.HttpHeaderName) && !string.IsNullOrWhiteSpace(correlationId))
            {
                httpContext.Response.Headers[options.HttpHeaderName] = correlationId;
            }
            return Task.CompletedTask;
        });
    }
}