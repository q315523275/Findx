using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Findx.Security;
using Findx.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Findx.Caching;
using Findx.Data;
using Microsoft.Extensions.Logging;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// Mvc功能信息提取实现
    /// </summary>
    public class MvcFunctionHandler : FunctionHandlerBase<MvcFunction>
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        private readonly ICacheProvider _cacheProvider;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        /// <param name="actionDescriptorCollectionProvider"></param>
        /// <param name="cacheProvider"></param>
        public MvcFunctionHandler(IServiceProvider serviceProvider, IFunctionStore<MvcFunction> store, ILogger<MvcFunctionHandler> logger, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, ICacheProvider cacheProvider) : base(serviceProvider, store, logger)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _cacheProvider = cacheProvider;
        }
        
        /// <summary>
        /// 获取程序集权限资源信息
        /// </summary>
        /// <returns></returns>
        protected override List<MvcFunction> GetFunctions(bool fromCache = true)
        {
            if (fromCache)
            {
                var cache = _cacheProvider.Get("memory");
                var functions = cache.Get<List<MvcFunction>>("function");
                if (functions is { Count: > 0 })
                {
                    return functions;
                }
            }

            var result = new List<MvcFunction>();
            var controllerActionList = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Cast<ControllerActionDescriptor>();
            // Controller循环
            foreach (var item in controllerActionList)
            {
                var routeValues = item.RouteValues;
                var controller = result.FirstOrDefault(x => x.IsController && x.Area == routeValues["area"] && x.Controller == item.ControllerName);
                if (controller == null)
                {
                    var typeInfo = item.ControllerTypeInfo;
                    var authorize = typeInfo.GetAttribute<AuthorizeAttribute>();
                    var typeAccessType = authorize == null
                                         ? FunctionAccessType.Anonymous
                                         : authorize.Roles.IsNullOrWhiteSpace()
                                         ? FunctionAccessType.Login
                                         : FunctionAccessType.RoleLimit;
                    controller = new MvcFunction()
                    {
                        Name = typeInfo.GetDescription(),
                        Area = routeValues.ContainsKey("area") ? routeValues["area"] : null,
                        Controller = item.ControllerName,
                        IsController = true,
                        AccessType = typeAccessType,
                        Roles = authorize?.Roles,
                        AuditOperationEnabled = !typeInfo.HasAttribute<DisableAuditingAttribute>(),
                        Id = Findx.Utils.SequentialGuid.Instance.Create()
                    };
                    result.Add(controller);
                }
                // Action
                var methodAuthorize = item.MethodInfo.GetAttribute<AuthorizeAttribute>();
                var methodAccessType = item.MethodInfo.HasAttribute<AllowAnonymousAttribute>()
                                                ? FunctionAccessType.Anonymous
                                                : methodAuthorize == null
                                                ? controller.AccessType
                                                : methodAuthorize.Roles.IsNullOrWhiteSpace()
                                                ? FunctionAccessType.Login : FunctionAccessType.RoleLimit;

                var actionRoles = methodAccessType == FunctionAccessType.RoleLimit
                                                ? methodAuthorize == null
                                                ? controller.Roles : methodAuthorize?.Roles
                                                : null;

                var auditOperationEnabled = !item.MethodInfo.HasAttribute<DisableAuditingAttribute>() && controller.AuditOperationEnabled;

                var function = new MvcFunction()
                {
                    Name = $"{controller.Name}-{item.MethodInfo.GetDescription()}",
                    Area = controller.Area,
                    Controller = controller.Controller,
                    Action = item.ActionName,
                    AccessType = methodAccessType,
                    IsController = false,
                    Roles = actionRoles,
                    AuditOperationEnabled = auditOperationEnabled,
                    Id = Findx.Utils.SequentialGuid.Instance.Create()
                };

                result.Add(function);
            }

            if (fromCache)
            {
                var cache = _cacheProvider.Get("memory");
                cache.Add("function", result);
            }

            return result;
        }
    }
}

