using Findx.Data;
using Findx.DependencyInjection;
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

        string area = endpoint.GetAreaName(),
            controller = endpoint.GetControllerName(),
            action = endpoint.GetActionName();
        var functionHandler = provider.GetService<IFunctionHandler>();
        if (functionHandler == null) return null;

        var function = functionHandler.GetFunction(area, controller, action);
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
        return endpoint.RoutePattern.RequiredValues["controller"]?.ToString();
    }

    /// <summary>
    ///     获取Action名
    /// </summary>
    public static string GetActionName(this RouteEndpoint endpoint)
    {
        return endpoint.RoutePattern.RequiredValues["action"]?.ToString();
    }
}