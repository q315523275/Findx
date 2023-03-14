using System.Collections.Generic;
using System.Linq;
using Findx.Security;
using Findx.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Findx.Caching;
using Findx.Data;
using Findx.Logging;
using Microsoft.AspNetCore.Mvc.Routing;

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
        /// <param name="logger"></param>
        /// <param name="store"></param>
        /// <param name="actionDescriptorCollectionProvider"></param>
        /// <param name="cacheProvider"></param>
        public MvcFunctionHandler(StartupLogger logger, IFunctionStore<MvcFunction> store, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, ICacheProvider cacheProvider) : base(store, logger)
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
                var cache = _cacheProvider.Get(CacheType.DefaultMemory);
                var functions = cache.Get<List<MvcFunction>>("function");
                if (functions is { Count: > 0 })
                {
                    return functions;
                }
            }
            
            // 耗时大致20毫秒左右
            var result = new List<MvcFunction>();
            var controllerActionList = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Cast<ControllerActionDescriptor>();
            // Controller循环
            foreach (var item in controllerActionList)
            {
                var routeValues = item.RouteValues;
                var area = routeValues.ContainsKey("area") ? routeValues["area"] : null;
                var controller = result.FirstOrDefault(x => x.IsController && x.Area == area && x.Controller == item.ControllerName);
                if (controller == null)
                {
                    var typeInfo = item.ControllerTypeInfo;
                    var authorize = typeInfo.GetAttribute<PreAuthorizeAttribute>()?? typeInfo.GetAttribute<AuthorizeAttribute>();
                    var typeAccessType = GetFunctionAccessType(authorize);
                    string authority = null;
                    if (typeAccessType is FunctionAccessType.AuthorityLimit or FunctionAccessType.RoleAuthorityLimit)
                        authority = (authorize as PreAuthorizeAttribute).Authority;
                    controller = new MvcFunction()
                    {
                        DisplayName = typeInfo.GetFullNameWithModule(),
                        Name = typeInfo.GetDescription(),
                        Area = area,
                        Controller = item.ControllerName,
                        IsController = true,
                        AccessType = typeAccessType,
                        Roles = authorize?.Roles,
                        Authority = authority,
                        AuditOperationEnabled = !typeInfo.HasAttribute<DisableAuditingAttribute>(),
                        Id = Utils.SequentialGuid.Instance.Create()
                    };
                    result.Add(controller);
                }
                // Action
                var methodHttp = item.MethodInfo.GetAttribute<HttpMethodAttribute>();
                var methodAuthorize = item.MethodInfo.GetAttribute<PreAuthorizeAttribute>()?? item.MethodInfo.GetAttribute<AuthorizeAttribute>();
                FunctionAccessType methodAccessType;
                if (item.MethodInfo.HasAttribute<AllowAnonymousAttribute>())
                {
                    methodAccessType = FunctionAccessType.Anonymous;
                }
                else if (methodAuthorize == null)
                {
                    methodAccessType = controller.AccessType;
                }
                else
                {
                    methodAccessType = GetFunctionAccessType(methodAuthorize);
                }
                // 角色资源
                string actionRoles = null;
                // 权限资源
                string actionAuthority = null;
                if (methodAccessType is FunctionAccessType.RoleLimit or FunctionAccessType.RoleAuthorityLimit)
                {
                    actionRoles = methodAuthorize == null ? controller.Roles : methodAuthorize?.Roles;
                }
                if (methodAccessType is FunctionAccessType.AuthorityLimit or FunctionAccessType.RoleAuthorityLimit)
                {
                    actionAuthority = methodAuthorize == null ? controller.Authority : (methodAuthorize as PreAuthorizeAttribute).Authority;
                }
                var auditOperationEnabled = !item.MethodInfo.HasAttribute<DisableAuditingAttribute>() && controller.AuditOperationEnabled;
                // 请求方法循环
                foreach (var httpMethod in methodHttp.HttpMethods.Distinct())
                {
                    var function = new MvcFunction()
                    {
                        Name = $"{controller.Name}-{item.MethodInfo.GetDescription()}",
                        DisplayName = item.DisplayName,
                        Area = controller.Area,
                        Controller = controller.Controller,
                        Action = item.ActionName,
                        AccessType = methodAccessType,
                        HttpMethod = httpMethod,
                        RouteTemplate = item.AttributeRouteInfo?.Template,
                        IsController = false,
                        Roles = actionRoles,
                        Authority = actionAuthority,
                        AuditOperationEnabled = auditOperationEnabled,
                        Id = Utils.SequentialGuid.Instance.Create()
                    };

                    result.Add(function);
                }
            }

            if (fromCache)
            {
                var cache = _cacheProvider.Get(CacheType.DefaultMemory);
                cache.Add("function", result);
            }

            return result;
        }

        /// <summary>
        /// 获取方法 访问类型
        /// </summary>
        /// <param name="authorizeAttribute"></param>
        /// <returns></returns>
        protected virtual FunctionAccessType GetFunctionAccessType(AuthorizeAttribute authorizeAttribute)
        {
            var typeAccessType = FunctionAccessType.Anonymous;
            // 匿名访问
            if (authorizeAttribute == null)
            {
                typeAccessType = FunctionAccessType.Anonymous;
            }
            else if (authorizeAttribute is PreAuthorizeAttribute preAuthorizeAttribute)
            {
                // 限定登陆访问
                if (preAuthorizeAttribute.Roles.IsNullOrWhiteSpace() && preAuthorizeAttribute.Authority.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.Login;
                // 限定角色、权限资源访问
                if (!preAuthorizeAttribute.Authority.IsNullOrWhiteSpace() && !preAuthorizeAttribute.Roles.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.RoleAuthorityLimit;
                // 限定权限资源访问
                else if (!preAuthorizeAttribute.Authority.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.AuthorityLimit;
                // 限定角色访问
                else if (!preAuthorizeAttribute.Roles.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.RoleLimit;
            }
            else
            {
                // 限定登陆访问
                if (authorizeAttribute.Roles.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.Login;
                // 限定角色访问
                if (!authorizeAttribute.Roles.IsNullOrWhiteSpace())
                    typeAccessType = FunctionAccessType.RoleLimit;
            }

            return typeAccessType;
        }
    }
}

