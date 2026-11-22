using Findx.Data;
using Findx.Extensions.Scheduler.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     仅调度端可用的接口守卫。
///     <para>执行端进程不注册数据库原语，也不提供调度端接口：命中本特性时直接返回 503，
///     而不是等到解析服务时抛出 DI 异常（那样会变成 500，运维看不出原因）。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class MasterRoleOnlyAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     执行前校验角色
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var options = LazyServices.Require<IOptions<SchedulerOptions>>(context.HttpContext.RequestServices);
        if (options?.Value.SchedulerEnabled == true) return;

        context.Result = new ObjectResult(
            CommonResult.Fail("503", "本节点未启用调度端（Findx:Scheduler:SchedulerEnabled=false），不提供调度端接口"))
        {
            StatusCode = 503
        };
    }
}
