using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace Findx.Security.Authorization
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class DefaultHttpContextExtensions
    {
        /// <summary>
        /// 获取正在执行的Action相关功能信息
        /// </summary>
        public static PermissionAccess GetExecutePermissionAccess(this DefaultHttpContext context)
        {
            IServiceProvider provider = context.RequestServices;

            // string path = context.Request.Path;
            string area = context.GetAreaName(),
                controller = context.GetControllerName(),
                action = context.GetActionName();
            IPermissionHandler permissionHandler = provider.GetService<IPermissionHandler>();
            Check.NotNull(permissionHandler, nameof(permissionHandler));

            PermissionAccess permissionAccess = permissionHandler.GetPermissionAccess(area, controller, action);

            return permissionAccess;
        }

        /// <summary>
        /// 获取Area名
        /// </summary>
        public static string GetAreaName(this DefaultHttpContext context)
        {
            string area = null;
            if (context.GetRouteData().Values.TryGetValue("area", out object value))
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
        public static string GetControllerName(this DefaultHttpContext context)
        {
            return context.GetRouteData().Values["controller"].ToString();
        }

        /// <summary>
        /// 获取Action名
        /// </summary>
        public static string GetActionName(this DefaultHttpContext context)
        {
            return context.GetRouteData().Values["action"].ToString();
        }
    }
}
