using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace Findx.Security.Authorization
{
    /// <summary>
    /// 端点扩展
    /// </summary>
    public static class EndpointExtensions
    {
        /// <summary>
        /// 获取正在执行的Action相关功能信息
        /// </summary>
        public static PermissionAccess GetExecutePermissionAccess(this RouteEndpoint endpoint, HttpContext context)
        {
            IServiceProvider provider = context.RequestServices;

            string area = endpoint.GetAreaName(),
                controller = endpoint.GetControllerName(),
                action = endpoint.GetActionName();
            IPermissionHandler permissionHandler = provider.GetService<IPermissionHandler>();
            Check.NotNull(permissionHandler, nameof(permissionHandler));

            PermissionAccess permissionAccess = permissionHandler.GetPermissionAccess(area, controller, action);

            return permissionAccess;
        }

        /// <summary>
        /// 获取Area名
        /// </summary>
        public static string GetAreaName(this RouteEndpoint endpoint)
        {
            string area = null;
            if (endpoint.RoutePattern.RequiredValues.TryGetValue("area", out object value))
            {
                area = (string)value;
                if (area.IsNullOrWhiteSpace())
                {
                    area = null;
                }
            }

            return area;
        }

        /// <summary>
        /// 获取Controller名
        /// </summary>
        public static string GetControllerName(this RouteEndpoint endpoint)
        {
            return endpoint.RoutePattern.RequiredValues["controller"].ToString();
        }

        /// <summary>
        /// 获取Action名
        /// </summary>
        public static string GetActionName(this RouteEndpoint endpoint)
        {
            return endpoint.RoutePattern.RequiredValues["action"].ToString();
        }
    }
}
