using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// 服务提供者定位器
    /// </summary>
    public sealed class ServiceLocator
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        public static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取单个泛型实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            Check.NotNull(ServiceProvider, nameof(ServiceProvider));

            IScopedServiceResolver scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
            if (scopedResolver != null && scopedResolver.ResolveEnabled)
            {
                return scopedResolver.GetService<T>() ?? default;
            }
            return ServiceProvider.GetService<T>() ?? default;
        }
        /// <summary>
        /// 获取单个实例对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetService(Type serviceType)
        {
            Check.NotNull(ServiceProvider, nameof(ServiceProvider));
            Check.NotNull(serviceType, nameof(serviceType));

            IScopedServiceResolver scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
            if (scopedResolver != null && scopedResolver.ResolveEnabled)
            {
                return scopedResolver.GetService(serviceType) ?? default;
            }
            return ServiceProvider.GetService(serviceType) ?? default;
        }
        /// <summary>
        /// 获取泛型实例集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>()
        {
            Check.NotNull(ServiceProvider, nameof(ServiceProvider));

            IScopedServiceResolver scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
            if (scopedResolver != null && scopedResolver.ResolveEnabled)
            {
                return scopedResolver.GetServices<T>() ?? default;
            }
            return ServiceProvider.GetServices<T>() ?? default;
        }
        /// <summary>
        /// 获取泛型实例对象集合
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IEnumerable<object> GetServices(Type serviceType)
        {
            Check.NotNull(ServiceProvider, nameof(ServiceProvider));
            Check.NotNull(serviceType, nameof(serviceType));

            IScopedServiceResolver scopedResolver = ServiceProvider.GetService<IScopedServiceResolver>();
            if (scopedResolver != null && scopedResolver.ResolveEnabled)
            {
                return scopedResolver.GetServices(serviceType) ?? default;
            }
            return ServiceProvider.GetServices(serviceType) ?? default;
        }

        /// <summary>
        /// 获取当前用户
        /// </summary>
        public static ClaimsPrincipal GetCurrentUser()
        {
            try
            {
                IPrincipal user = GetService<IPrincipal>();
                return user as ClaimsPrincipal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
