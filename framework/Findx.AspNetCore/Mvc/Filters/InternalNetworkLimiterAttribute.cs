using System;
using System.Threading.Tasks;
using Findx.AspNetCore.Extensions;
using Findx.Data;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
            await next();
        else
            context.Result = new JsonResult(CommonResult.Fail("403", $"The network ({ipv4Address}) request has been intercepted."));
    }
}