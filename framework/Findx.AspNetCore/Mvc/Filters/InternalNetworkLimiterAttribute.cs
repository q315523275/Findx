using System;
using System.Threading.Tasks;
using Findx.AspNetCore.Events;
using Findx.AspNetCore.Extensions;
using Findx.Data;
using Findx.Events;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     私有网络访问限制
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class InternalNetworkLimiterAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     Action执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipv4Address = context.HttpContext.GetClientIp();
        if (NetUtility.IsInternalIp(ipv4Address))
        {
            await next();
        }
        else
        {
            var provider = context.HttpContext.RequestServices;
            // 触发统计计算
            var eventBus = provider.GetService<IEventBus>();
            var eventData = new InternalNetworkLimitedEvent
            {
                TriggerTime = DateTime.Now,
                ClientIpAddress = context.HttpContext.GetClientIp(), 
                UserAgent = context.HttpContext.Request.GetUserAgentString()
            };
            await eventBus.PublishAsync(eventData, context.HttpContext.RequestAborted);
            
            context.Result = new JsonResult(CommonResult.Fail("403", $"The network ({ipv4Address}) request has been intercepted."));
        }
    }
}