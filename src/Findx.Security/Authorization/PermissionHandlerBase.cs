using Findx.Extensions;
using Findx.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// 应用程序Action集合提供器
        /// </summary>
        public IActionDescriptorCollectionProvider ActionDescriptorCollectionProvider { set; get; }

        /// <summary>
        /// 从程序集中获取功能信息
        /// </summary>
        public async Task InitializeAsync()
        {
            List<Permission> permissions = GetPermissions();
            await SyncToStoreAsync(permissions);
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
            var jsTime = DateTime.Now;
            var list = ActionDescriptorCollectionProvider.ActionDescriptors.Items.Cast<ControllerActionDescriptor>();
            Debug.WriteLine($"----------------初始化ActionDescriptors资源耗时:{(DateTime.Now - jsTime).TotalMilliseconds}ms");
            jsTime = DateTime.Now;
            foreach (var item in list)
            {
                var routeValues = item.RouteValues;
                var area = routeValues["area"];
                var typePermission = _permissions.FirstOrDefault(x => x.IsController == true && x.Area == area && x.Controller == item.ControllerName);
                if (typePermission == null)
                {
                    var typeInfo = item.ControllerTypeInfo;
                    AuthorizeAttribute authorize = typeInfo.GetAttribute<AuthorizeAttribute>();
                    PermiessionAccessType typeAccessType = authorize == null ? PermiessionAccessType.Anonymous :
                           authorize.Roles.IsNullOrWhiteSpace() ? PermiessionAccessType.Login : PermiessionAccessType.RoleLimit;
                    typePermission = new Permission()
                    {
                        Name = typeInfo.GetDescription(),
                        Area = routeValues["area"],
                        Controller = item.ControllerName,
                        IsController = true,
                        AccessType = typeAccessType,
                        Roles = authorize?.Roles
                    };
                    _permissions.Add(typePermission);
                }

                // Action
                AuthorizeAttribute methodAuthorize = item.MethodInfo.GetAttribute<AuthorizeAttribute>();
                PermiessionAccessType methodAccessType = item.MethodInfo.HasAttribute<AllowAnonymousAttribute>()
                                                ? PermiessionAccessType.Anonymous
                                                : methodAuthorize == null
                                                ? typePermission.AccessType
                                                : methodAuthorize.Roles.IsNullOrWhiteSpace()
                                                ? PermiessionAccessType.Login : PermiessionAccessType.RoleLimit;
                string actionRoles = methodAccessType == PermiessionAccessType.RoleLimit
                                                ? methodAuthorize == null
                                                ? typePermission.Roles : methodAuthorize?.Roles
                                                : null;
                Permission MemberPermission = new()
                {
                    Name = $"{typePermission.Name}-{item.MethodInfo.GetDescription()}",
                    Area = typePermission.Area,
                    Controller = typePermission.Controller,
                    Action = item.ActionName,
                    AccessType = methodAccessType,
                    IsController = false,
                    Roles = actionRoles
                };

                _permissions.Add(MemberPermission);
            }
            Debug.WriteLine($"----------------初始化接口资源耗时:{(DateTime.Now - jsTime).TotalMilliseconds}ms");
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
