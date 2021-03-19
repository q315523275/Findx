using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// 服务提供者定位器
    /// </summary>
    public class ServiceLocator
    {
        public static IServiceProvider Instance { get; set; }

        public static T GetService<T>()
        {
            Check.NotNull(Instance, nameof(Instance));

            IScopedServiceResolver scopedResolver = Instance.GetService<IScopedServiceResolver>();
            if (scopedResolver != null && scopedResolver.ResolveEnabled)
            {
                return scopedResolver.GetService<T>();
            }
            return Instance.GetService<T>() ?? default;
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
