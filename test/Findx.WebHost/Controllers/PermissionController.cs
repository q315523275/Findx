﻿using System.Linq;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Findx.WebHost.Controllers;

[Area("app")]
[Route("[area]/auth")]
//[Authorize(Roles = "admin")]
public class PermissionController : PermissionControllerBase
{
    /// <summary>
    ///     权限数据查询示例接口
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    [HttpGet("/permission/list")]
    public CommonResult PermissionList([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(store.GetFromDatabase());
    }

    /// <summary>
    ///     权限数据查询示例接口
    /// </summary>
    /// <param name="actionProvider"></param>
    /// <returns></returns>
    [HttpGet("/permission/action")]
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
    [HttpGet("verify")]
    [Authorize(Policy = FunctionRequirement.Policy, Roles = "admin")]
    public CommonResult VerifyPermission([FromServices] IFunctionStore<MvcFunction> store)
    {
        return CommonResult.Success(store.GetFromDatabase());
    }
}