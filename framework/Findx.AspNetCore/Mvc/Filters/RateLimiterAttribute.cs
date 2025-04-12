using System;
using System.Threading.Tasks;
using Findx.AspNetCore.Extensions;
using Findx.Caching;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     限速过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RateLimiterAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     业务标识
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///     限定请求时长
    /// </summary>
    public string Period { get; set; } = "30s";

    /// <summary>
    ///     限定时长内请求次数
    /// </summary>
    public int Limit { get; set; } = 30;

    /// <summary>
    ///     是否分布式
    /// </summary>
    public bool IsDistributed { set; get; } = false;
    
    /// <summary>
    ///     限速类型
    /// </summary>
    public RateLimitType Type { get; set; } = RateLimitType.Ip;

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        Check.NotNull(context, nameof(context));

        // 本地缓存相当于是字典，存储使用均为同一个对象
        // 如果使用分布式缓存，需做好计数器

        var cacheFactory = context.HttpContext.RequestServices.GetRequiredService<ICacheFactory>();
        var cacheType = IsDistributed ? CacheType.DefaultRedis : CacheType.DefaultMemory;
        var cache = cacheFactory.Create(cacheType);
        var rateLimitKey = GetRateLimitKey(context);
        var rateLimitValue = await cache.IncrementAsync(rateLimitKey, expire: TimeSpanUtility.ToTimeSpan(Period));
        if (rateLimitValue > Limit)
            context.Result = new JsonResult(CommonResult.Fail("429", $"Too many requests in {Period}. Try again later."));
        else
            await next();
    }

    private string GetRateLimitKey(ActionExecutingContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetCurrentUser();

        var clientId = string.Empty;

        if (currentUser.Identity != null && Type == RateLimitType.User && currentUser.Identity.IsAuthenticated)
            clientId = $"{currentUser.Identity.GetUserId()}_";

        if (Type == RateLimitType.Ip)
            clientId = $"{context.HttpContext.GetClientIp()}_";

        return Key.IsNullOrWhiteSpace() ? $"RL:{clientId}{context.HttpContext.Request.Path}" : $"RL:{clientId}{Key}";
    }
}

/// <summary>
///     锁类型
/// </summary>
public enum RateLimitType
{
    /// <summary>
    ///     用户级别限速,同一用户只能在指定速率规则进行请求
    /// </summary>
    User = 0,

    /// <summary>
    ///     IP级别限速,同一IP只能在指定速率规则进行请求
    /// </summary>
    Ip = 1,

    /// <summary>
    ///     全局级别限速，全局只能在指定速率规则进行请求
    /// </summary>
    Global = 2
}