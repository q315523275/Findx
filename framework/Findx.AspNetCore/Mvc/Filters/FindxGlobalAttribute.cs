using System;
using System.Linq;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     全局过滤器
///     <para>模型验证,不通过返回json</para>
///     <para>登录后租户赋值</para>
/// </summary>
public class FindxGlobalAttribute : IActionFilter
{
    /// <summary>
    ///     Action执行前
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // 模型判断
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                                .Where(e => e.Value != null && e.Value.Errors.Any())
                                .Select(e => new ErrorMember { ErrorMemberName = e.Key, ErrorMessage = e.Value.Errors.Select(x => x.ErrorMessage).ExpandAndToString() });

            context.Result = new JsonResult(CommonResult.Fail("4001", errors.Select(x => x.ErrorMessage).ExpandAndToString(Utils.Common.Line)));
        }

        // 租户赋值
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
        if (currentUser is { IsAuthenticated: true } && !currentUser.TenantId.IsNullOrWhiteSpace())
            TenantManager.Current = currentUser.TenantId.CastTo<Guid>();

        // 刷新Token
        // 已迁移组建内置实现
    }

    /// <summary>
    ///     Action执行后
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        TenantManager.Current = Guid.Empty;
    }
}