using Findx.AspNetCore.Extensions;
using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Threading;
using Findx.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 限速过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimiterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 业务标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 限定请求时长
        /// </summary>
        public string Period { get; set; } = "30s";
        /// <summary>
        /// 限定时长内请求次数
        /// </summary>
        public int Limit { get; set; } = 30;
        /// <summary>
        /// 限速类型
        /// </summary>
        public RateLimitType Type { get; set; } = RateLimitType.IP;
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Check.NotNull(context, nameof(context));

            // 本地缓存相当于是字典，存储使用均为同一个对象
            // 如果使用分布式缓存，需做好计数器

            ICancellationTokenProvider cancellationTokenProvider = context.HttpContext.RequestServices.GetService<ICancellationTokenProvider>();
            ICache cache = context.HttpContext.RequestServices.GetService<ICacheProvider>().Get(CacheType.DefaultMemory);
            string rateLimitKey = GetRateLimitKey(context);
            AtomicInteger atomic = await cache.GetAsync<AtomicInteger>(rateLimitKey, cancellationTokenProvider.Token);
            if (atomic == default)
            {
                atomic = new AtomicInteger(Limit);
                await cache.AddAsync(rateLimitKey, atomic, expiration: DateTimeUtils.ToTimeSpan(Period), token: cancellationTokenProvider.Token);
            }
            if (atomic.DecrementAndGet() < 0)
            {
                context.Result = new JsonResult(CommonResult.Fail("4029", "Frequent network requests"));
            }
            else
            {
                await next();
            }
        }
        private string GetRateLimitKey(ActionExecutingContext context)
        {
            var currentUser = context.HttpContext.RequestServices.GetCurrentUser();
            var clientId = string.Empty;
            if (Type == RateLimitType.User && currentUser.Identity.IsAuthenticated)
                clientId = $"{currentUser.Identity.GetUserId()}_";
            if (Type == RateLimitType.IP)
                clientId = $"{context.HttpContext.GetClientIp()}_";
            return Key.IsNullOrWhiteSpace() ? $"RL:{clientId}{context.HttpContext.Request.Path}" : $"RL:{clientId}{Key}";
        }
    }
    /// <summary>
    /// 锁类型
    /// </summary>
    public enum RateLimitType
    {
        /// <summary>
        /// 用户级别限速,同一用户只能在指定速率规则进行请求
        /// </summary>
        User = 0,

        /// <summary>
        /// IP级别限速,同一IP只能在指定速率规则进行请求
        /// </summary>
        IP = 1,

        /// <summary>
        /// 全局级别限速，全局只能在指定速率规则进行请求
        /// </summary>
        Global = 2
    }
}
