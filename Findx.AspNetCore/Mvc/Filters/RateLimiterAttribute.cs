using Microsoft.AspNetCore.Mvc.Filters;
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
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Check.NotNull(context, nameof(context));

            //操作执行前做的事情
            await next();
            //操作执行后做的事情
        }
    }
}
