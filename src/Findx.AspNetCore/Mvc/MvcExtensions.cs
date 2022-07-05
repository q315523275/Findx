using System;
using Findx.DependencyInjection;
using Findx.Security;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// MVC相关扩展方法
    /// </summary>
    public static class MvcExtensions
    {
        /// <summary>
        /// 获取正在执行的Action的相关功能信息
        /// </summary>
        public static IFunction GetExecuteFunction(this ActionContext context)
        {
            Check.NotNull(context, nameof(context));

            var provider = context.HttpContext.RequestServices;
            var dict = provider.GetService<ScopedDictionary>();
            if (dict.Function != null)
            {
                return dict.Function;
            }
            string area = context.GetAreaName();
            string controller = context.GetControllerName();
            string action = context.GetActionName();
            var functionHandler = provider.GetService<IFunctionHandler>();
            if (functionHandler == null)
            {
                return null;
            }
            var function = functionHandler.GetFunction(area, controller, action);
            if (function != null)
            {
                dict.Function = function;
            }
            return function;
        }

        /// <summary>
        /// 获取Area名
        /// </summary>
        public static string GetAreaName(this ActionContext context)
        {
            Check.NotNull(context, nameof(context));

            string area = null;
            if (context.RouteData.Values.TryGetValue("area", out object value))
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
        public static string GetControllerName(this ActionContext context)
        {
            Check.NotNull(context, nameof(context));

            return context.RouteData.Values["controller"].ToString();
        }

        /// <summary>
        /// 获取Action名
        /// </summary>
        public static string GetActionName(this ActionContext context)
        {
            Check.NotNull(context, nameof(context));
            return context.RouteData.Values["action"].ToString();
        }
    }
}

