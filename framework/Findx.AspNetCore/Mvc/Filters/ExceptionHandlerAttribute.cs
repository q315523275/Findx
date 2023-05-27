using Findx.Data;
using Findx.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     异常处理过滤器
/// </summary>
public class ExceptionHandlerAttribute : ExceptionFilterAttribute
{
    /// <summary>
    ///     异常处理
    /// </summary>
    /// <param name="context">异常上下文</param>
    public override void OnException(ExceptionContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<ExceptionHandlerAttribute>>();

        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = 200;

        if (context.Exception is FindxException findxException)
        {
            context.Result = new JsonResult(CommonResult.Fail(findxException.ErrorCode, findxException.Message));
        }
        else
        {
            logger?.LogError(context.Exception, "WebApi全局异常");
            context.Result = new JsonResult(CommonResult.Fail("500", "系统开小差,请稍后再试"));
        }
    }
}