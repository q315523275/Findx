﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Findx.AspNetCore.Extensions;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Security;
using Findx.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     操作审计拦截器，负责发起并记录功能的操作日志
/// </summary>
public sealed class AuditOperationAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     Action环切
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var provider = context.HttpContext.RequestServices;
        var httpContext = context.HttpContext;
        // 功能识别
        var function = context.GetExecuteFunction();
        if (function == null) return;
        var serializer = provider.GetRequiredService<IJsonSerializer>();
        var options = provider.GetRequiredService<IOptions<AuditingOptions>>();
        var dict = provider.GetRequiredService<ScopedDictionary>();

        // 数据权限有效角色，即有当前功能权限的角色
        var functionAuthorization = provider.GetRequiredService<IFunctionAuthorization>();
        var roleName = functionAuthorization.GetOkRoles(function, context.HttpContext.User);
        dict.DataAuthValidRoleNames = roleName;

        // 审计数据初始化
        var operation = new AuditOperationEntry
        {
            FunctionName = function.Name,
            ClientIpAddress = httpContext.GetClientIp(),
            UserAgent = httpContext.Request.GetUserAgentString(),
            ExecutionTime = DateTime.Now
        };

        // 认证参数
        if (httpContext.User.Identity is { IsAuthenticated: true } and ClaimsIdentity identity)
        {
            operation.UserId = identity.GetUserId();
            operation.UserName = identity.GetUserName();
            operation.NickName = identity.GetNickname();
            operation.TenantId = identity.GetTenantId();
        }

        dict.AuditOperation = operation;
        
        // 执行业务
        var actionContext = await next();
        
        // 审计执行判断
        if (!options.Value.Enabled || !function.AuditOperationEnabled) return;
        
        operation.EndedTime = DateTime.Now;
        operation.Exception = actionContext.Exception;

        // Mvc参数
        dict.AuditOperation.ExtraObject.Add("http.url", context.HttpContext.Request.GetDisplayUrl());
        dict.AuditOperation.ExtraObject.Add("http.path", context.HttpContext.Request.Path);
        dict.AuditOperation.ExtraObject.Add("http.method", context.HttpContext.Request.Method);
        dict.AuditOperation.ExtraObject.Add("http.status_code", actionContext.HttpContext.Response.StatusCode.ToString());

        // 参数报文
        if (options.Value.ExtractRequestBody)
            dict.AuditOperation.ExtraObject.Add("http.request", SerializeConvertArguments(context.ActionArguments, serializer));

        // 返回结果
        if (options.Value.ExtractResponseBody)
        {
            if (actionContext.Exception is FindxException findxException)
                dict.AuditOperation.ExtraObject.Add("http.response", serializer.Serialize(CommonResult.Fail(findxException.ErrorCode, findxException.ErrorMessage)));
            else
                dict.AuditOperation.ExtraObject.Add("http.response", SerializeConvertResponse(actionContext.Result, serializer));
        }

        // 存储
        var store = provider.GetService<IAuditStore>();
        if (store != null) 
            await store.SaveAsync(dict.AuditOperation, context.HttpContext.RequestAborted);
    }

    // 过滤类型
    private static readonly List<Type> IgnoredTypes = [typeof(IFormFile), typeof(FromServicesAttribute), typeof(Stream)];
    
    /// <summary>
    ///     转换参数报文
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    private static string SerializeConvertArguments(IDictionary<string, object> arguments, IJsonSerializer serializer)
    {
        try
        {
            if (arguments.IsNullOrEmpty()) return string.Empty;

            var dictionary = new Dictionary<string, object>();

            foreach (var argument in arguments)
                if (argument.Value != null && IgnoredTypes.Any(t => t.IsInstanceOfType(argument.Value)))
                    dictionary[argument.Key] = null;
                else
                    dictionary[argument.Key] = argument.Value;

            return serializer.Serialize(dictionary);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     转换结果报文
    /// </summary>
    /// <param name="result"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    private static string SerializeConvertResponse(IActionResult result, IJsonSerializer serializer)
    {
        try
        {
            switch (result)
            {
                case ObjectResult objectResult:
                    return serializer.Serialize(objectResult.Value);
                case JsonResult jsonResult:
                    return serializer.Serialize(jsonResult.Value);
                case ContentResult contentResult:
                    return contentResult.Content;
            }
        }
        catch
        {
            // ignored
        }

        return string.Empty;
    }
}