using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Security.Authorization;

/// <summary>
///     端点扩展
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    ///     获取正在执行的Action相关功能信息
    /// </summary>
    public static IFunction GetExecuteFunction(this RouteEndpoint endpoint, HttpContext context)
    {
        var provider = context.RequestServices;
        var dict = provider.GetRequiredService<ScopedDictionary>();
        if (dict.Function != null) return dict.Function;

        var area = endpoint.GetAreaName();
        var controller = endpoint.GetControllerName();
        var action = endpoint.GetActionName();
        var method = context.Request.Method;
            
        var functionHandler = provider.GetService<IFunctionHandler>();
        if (functionHandler == null) return null;

        var function = functionHandler.GetFunction(area, controller, action, method);
        if (function != null) dict.Function = function;

        return function;
    }

    /// <summary>
    ///     获取Area名
    /// </summary>
    public static string GetAreaName(this RouteEndpoint endpoint)
    {
        string area = null;
        if (endpoint.RoutePattern.RequiredValues.TryGetValue("area", out var value))
        {
            area = (string)value;
            if (area.IsNullOrWhiteSpace()) area = null;
        }

        return area;
    }

    /// <summary>
    ///     获取Controller名
    /// </summary>
    public static string GetControllerName(this RouteEndpoint endpoint)
    {
        //  最小API/健康检查等端点无 controller 路由值，索引器会抛 KeyNotFoundException，用 TryGetValue 兜底
        return endpoint.RoutePattern.RequiredValues.TryGetValue("controller", out var controller) ? controller?.ToString() : null;
    }

    /// <summary>
    ///     获取Action名
    /// </summary>
    public static string GetActionName(this RouteEndpoint endpoint)
    {
        //  最小API/健康检查等端点无 action 路由值，索引器会抛 KeyNotFoundException，用 TryGetValue 兜底
        return endpoint.RoutePattern.RequiredValues.TryGetValue("action", out var action) ? action?.ToString() : null;
    }
}