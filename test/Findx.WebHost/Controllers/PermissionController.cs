using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Findx.WebHost.Controllers;

/// <summary>
///     认证授权
/// </summary>
[Route("api/authorize")]
[Description("认证授权"), Tags("认证授权")]
public class PermissionController : ApiControllerBase
{
    /// <summary>
    ///     权限数据查询示例接口
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<CommonResult> PermissionList([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(await store.QueryFromDatabaseAsync());
    }

    /// <summary>
    ///     权限数据查询示例接口
    /// </summary>
    /// <param name="actionProvider"></param>
    /// <returns></returns>
    [HttpGet("actions")]
    public CommonResult ActionList([FromServices] IActionDescriptorCollectionProvider actionProvider)
    {
        var actionDesc = actionProvider.ActionDescriptors.Items.Cast<ControllerActionDescriptor>().Select(x => new
        {
            x.ControllerName,
            x.ActionName,
            x.DisplayName,
            RouteTemplate = x.AttributeRouteInfo?.Template,
            ActionRoles = x.MethodInfo.GetAttribute<AuthorizeAttribute>()?.Roles,
            ControllerRoles = x.ControllerTypeInfo.GetAttribute<AuthorizeAttribute>()?.Roles,
            ActionId = x.Id,
            x.RouteValues,
            Parameters = x.Parameters.Select(z => new
            {
                z.Name,
                TypeName = z.ParameterType.Name
            })
        });
        return CommonResult.Success(actionDesc);
    }

    /// <summary>
    ///     接口权限校验接口
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    [HttpGet("authorize")]
    [Authorize(Policy = FunctionRequirement.Policy, Roles = "admin")]
    public async Task<CommonResult> AuthorizePermission([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(await store.QueryFromDatabaseAsync());
    }
    
    /// <summary>
    ///     接口权限资源校验接口
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    [HttpGet("preAuthorize")]
    [PreAuthorize(Policy = FunctionRequirement.Policy, Authority = "Sys:Actions")]
    public async Task<CommonResult> PreAuthorizePermission([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(await store.QueryFromDatabaseAsync());
    }
}