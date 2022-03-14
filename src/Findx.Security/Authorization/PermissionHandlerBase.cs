using Findx.Extensions;
using Findx.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限资源处理器资源
    /// </summary>
    public abstract class PermissionHandlerBase : IPermissionHandler
    {
        /// <summary>
        /// 权限资源信息
        /// </summary>
        private List<Permission> _permissions;

        /// <summary>
        /// 资源权限信息
        /// </summary>
        private readonly ConcurrentDictionary<string, PermissionAccess> _permissionAccess;

        /// <summary>
        /// Ctor
        /// </summary>
        public PermissionHandlerBase()
        {
            _permissions = new List<Permission>();
            _permissionAccess = new ConcurrentDictionary<string, PermissionAccess>();
        }

        /// <summary>
        /// 应用程序部分管理
        /// </summary>
        public ApplicationPartManager PartManager { get; set; }

        /// <summary>
        /// 方法查询器
        /// </summary>
        public IMethodInfoFinder MethodInfoFinder { get; set; }

        /// <summary>
        /// 服务提供器
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 从程序集中获取功能信息
        /// </summary>
        public void Initialize()
        {
            Check.NotNull(PartManager, nameof(PartManager));
            Check.NotNull(MethodInfoFinder, nameof(MethodInfoFinder));
            Check.NotNull(ServiceProvider, nameof(ServiceProvider));

            List<Permission> permissions = GetPermissions();
            SyncToStoreAsync(permissions).ConfigureAwait(false).GetAwaiter();
            RefreshCache();
        }

        /// <summary>
        /// 清空资源信息缓存
        /// </summary>
        public void ClearCache()
        {
            _permissions.Clear();
            _permissionAccess.Clear();
        }

        /// <summary>
        /// 刷新资源信息缓存
        /// </summary>
        public void RefreshCache()
        {
            var list = GetFromStoreAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            ClearCache();

            _permissions.AddRange(list);

            foreach (Permission permission in _permissions)
            {
                var roles = permission.Roles.IsNullOrWhiteSpace() ? new string[] { } : permission.Roles.Split(",");
                _permissionAccess[$"{permission.Area}-{permission.Controller}-{permission.Action}"] = new PermissionAccess(permission.AccessType, roles);
            }
        }

        /// <summary>
        /// 获取程序集权限资源信息
        /// </summary>
        /// <returns></returns>
        protected virtual List<Permission> GetPermissions()
        {
            ControllerFeature controllerFeature = new();
            PartManager.PopulateFeature(controllerFeature);
            IList<TypeInfo> controllerTypes = controllerFeature.Controllers;
            foreach (TypeInfo typeInfo in controllerTypes)
            {
                if (typeInfo.IsAbstract || !typeInfo.IsPublic)
                {
                    continue;
                }
                AuthorizeAttribute authorize = typeInfo.GetAttribute<AuthorizeAttribute>();
                PermiessionAccessType typeAccessType = authorize == null ? PermiessionAccessType.Anonymous :
                       authorize.Roles.IsNullOrWhiteSpace() ? PermiessionAccessType.Login : PermiessionAccessType.RoleLimit;
                Permission typePermission = new()
                {
                    Name = typeInfo.GetDescription(),
                    Area = GetArea(typeInfo),
                    Controller = typeInfo.Name.Replace("ControllerBase", string.Empty).Replace("Controller", string.Empty),
                    IsController = true,
                    AccessType = typeAccessType,
                    Roles = authorize?.Roles
                };
                _permissions.Add(typePermission);

                var methods = MethodInfoFinder.FindAll(typeInfo);
                foreach (MemberInfo method in methods)
                {
                    if (!method.CustomAttributes.Any(m =>
                            m.AttributeType == typeof(HttpGetAttribute)
                            || m.AttributeType == typeof(HttpPostAttribute)
                            || m.AttributeType == typeof(HttpPutAttribute)
                            || m.AttributeType == typeof(HttpOptionsAttribute)
                            || m.AttributeType == typeof(HttpHeadAttribute)
                            || m.AttributeType == typeof(HttpPatchAttribute)
                            || m.AttributeType == typeof(HttpDeleteAttribute)))
                    {
                        continue;
                    }

                    AuthorizeAttribute methodAuthorize = method.GetAttribute<AuthorizeAttribute>();
                    PermiessionAccessType methodAccessType = method.HasAttribute<AllowAnonymousAttribute>()
                                                    ? PermiessionAccessType.Anonymous
                                                    : methodAuthorize == null
                                                    ? typeAccessType
                                                    : methodAuthorize.Roles.IsNullOrWhiteSpace()
                                                    ? PermiessionAccessType.Login : PermiessionAccessType.RoleLimit;
                    string actionRoles = methodAccessType == PermiessionAccessType.RoleLimit
                                                    ? methodAuthorize == null
                                                    ? authorize?.Roles : methodAuthorize?.Roles
                                                    : null;
                    Permission MemberPermission = new()
                    {
                        Name = $"{typePermission.Name}-{method.GetDescription()}",
                        Area = typePermission.Area,
                        Controller = typePermission.Controller,
                        Action = method.Name,
                        AccessType = methodAccessType,
                        IsController = false,
                        Roles = actionRoles
                    };

                    _permissions.Add(MemberPermission);
                }
            }
            return _permissions;
        }

        /// <summary>
        /// 从类型中获取功能的区域信息
        /// </summary>
        private static string GetArea(MemberInfo type)
        {
            AreaAttribute attribute = type.GetAttribute<AreaAttribute>();
            return attribute?.RouteValue;
        }

        /// <summary>
        /// 获取权限信息
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Permission GetPermission(string area, string controller, string action)
        {
            return _permissions.FirstOrDefault(it => it.Area == area && it.Controller == controller && it.Action == action);
        }

        /// <summary>
        /// 获取权限访问信息
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public PermissionAccess GetPermissionAccess(string area, string controller, string action)
        {
            _permissionAccess.TryGetValue($"{area}-{controller}-{action}", out PermissionAccess result);
            return result;
        }

        /// <summary>
        /// 同步存储程序集获取的资源权限信息
        /// </summary>
        /// <param name="permissions"></param>
        protected abstract Task SyncToStoreAsync(List<Permission> permissions);

        /// <summary>
        /// 从存储中获取最新资源权限信息
        /// </summary>
        /// <returns></returns>
        protected abstract Task<List<Permission>> GetFromStoreAsync();
    }
}
