﻿using Findx.AspNetCore.Extensions;
using Findx.Data;
using Findx.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 私有网络访问限制
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PrivateNetworkLimiterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Action执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ipv4Address = context.HttpContext.GetClientIp();
            if (NetUtils.IsPrivateNetwork(ipv4Address))
            {
                await next();
            }
            else
            {
                context.Result = new JsonResult(CommonResult.Fail("4003", $"Network request interception; IP({ipv4Address})"));
            }
        }
    }
}
