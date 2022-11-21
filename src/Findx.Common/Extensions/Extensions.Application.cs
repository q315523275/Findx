using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Messaging;

namespace Findx.Extensions
{
    /// <summary>
    /// 扩展 - 应用
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// 推送事件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="applicationEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task PublishEventAsync(this IApplicationContext context, IApplicationEvent applicationEvent, CancellationToken cancellationToken = default)
        {
            return ServiceLocator.GetService<IApplicationEventPublisher>()?.PublishAsync(applicationEvent, cancellationToken);
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetService(Type serviceType)
        {
            return ServiceLocator.GetService(serviceType);
        }

        /// <summary>
        /// 获取服务集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>()
        {
            return ServiceLocator.GetServices<T>();
        }

        /// <summary>
        /// 获取服务集合
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceLocator.GetServices(serviceType);
        }
    }
}

