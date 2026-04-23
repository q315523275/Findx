using System.Threading.Tasks;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Microsoft.AspNetCore.Http;

namespace Findx.AspNetCore.Mvc.Middlewares;

/// <summary>
///     租户上下文中间件
///     从 JWT Token 中提取 TenantId 并设置到 TenantManager
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="next"></param>
    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///    中间件执行方法
    /// </summary>
    /// <param name="context"></param>
    public async Task InvokeAsync(HttpContext context)
    {
        //  从用户 Claims 中提取 TenantId
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst(ClaimTypes.TenantId);
            
            if (tenantIdClaim != null && tenantIdClaim.Value.IsNotNullOrWhiteSpace())
            {
                //  设置当前租户上下文
                TenantManager.Current = tenantIdClaim.Value;
            }
        }

        try
        {
            await _next(context);
        }
        finally
        {
            //  请求结束后清理，避免内存泄漏
            TenantManager.Current = null;
        }
    }
}
