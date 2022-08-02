using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// Findx-自动注入模块
    /// </summary>
    [Description("Findx-自动注入模块")]
    public class DependencyModule : FindxModule
    {
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        /// 排序
        /// </summary>
        public override int Order => 10;

        /// <summary>
        /// 配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 查找所有自动注册的服务实现类型
            IDependencyTypeFinder dependencyTypeFinder = services.GetOrAddTypeFinder<IDependencyTypeFinder>(assemblyFinder => new DependencyTypeFinder(assemblyFinder));

            var dependencyTypes = dependencyTypeFinder.FindAll();
            foreach (Type dependencyType in dependencyTypes)
            {
                ConfigureServices(services, dependencyType);
            }

            return services;
        }

        /// <summary>
        /// 启用模块
        /// </summary>
        /// <param name="provider"></param>
        public override void UseModule(IServiceProvider provider)
        {
            ServiceLocator.ServiceProvider = provider;

            base.UseModule(provider);
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="implementationType"></param>
        protected virtual void ConfigureServices(IServiceCollection services, Type implementationType)
        {
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                return;
            }
            ServiceLifetime? lifetime = GetLifetimeOrNull(implementationType);
            if (lifetime == null)
            {
                return;
            }
            DependencyAttribute dependencyAttribute = implementationType.GetAttribute<DependencyAttribute>();
            Type[] serviceTypes = GetImplementedInterfaces(implementationType);

            // 服务数量为0时注册自身
            if (serviceTypes.Length == 0)
            {
                services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime.Value));
                return;
            }

            // 服务实现显示要求注册身处时，注册自身并且继续注册接口
            if (dependencyAttribute?.AddSelf == true)
            {
                services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime.Value));
            }

            // 注册服务
            for (int i = 0; i < serviceTypes.Length; i++)
            {
                Type serviceType = serviceTypes[i];
                ServiceDescriptor descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime.Value);
                if (lifetime.Value == ServiceLifetime.Transient)
                {
                    services.TryAddEnumerable(descriptor);
                    continue;
                }

                bool multiple = serviceType.HasAttribute<MultipleDependencyAttribute>();
                if (i == 0)
                {
                    if (multiple)
                    {
                        services.Add(descriptor);
                    }
                    else
                    {
                        AddSingleService(services, descriptor, dependencyAttribute);
                    }
                }
                else
                {
                    if (multiple)
                    {
                        services.Add(descriptor);
                    }
                    else
                    {
                        // 有多个接口，后边的接口注册使用第一个接口的实例，保证同个实现类的多个接口获得同一实例
                        Type firstServiceType = serviceTypes[0];
                        descriptor = new ServiceDescriptor(serviceType, provider => provider.GetService(firstServiceType), lifetime.Value);
                        AddSingleService(services, descriptor, dependencyAttribute);
                    }
                }
            }
        }

        /// <summary>
        /// 获取服务生命周期
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual ServiceLifetime? GetLifetimeOrNull(Type type)
        {
            DependencyAttribute attribute = type.GetAttribute<DependencyAttribute>();
            if (attribute != null)
            {
                return attribute.Lifetime;
            }

            if (type.IsDeriveClassFrom<ITransientDependency>())
            {
                return ServiceLifetime.Transient;
            }

            if (type.IsDeriveClassFrom<IScopeDependency>())
            {
                return ServiceLifetime.Scoped;
            }

            if (type.IsDeriveClassFrom<ISingletonDependency>())
            {
                return ServiceLifetime.Singleton;
            }

            return null;
        }

        /// <summary>
        /// 获取依赖接口类型集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Type[] GetImplementedInterfaces(Type type)
        {
            Type[] exceptInterfaces = { typeof(IDisposable) };
            Type[] interfaceTypes = type.GetInterfaces().Where(t => !exceptInterfaces.Contains(t) && !t.HasAttribute<IgnoreDependencyAttribute>()).ToArray();
            for (int index = 0; index < interfaceTypes.Length; index++)
            {
                Type interfaceType = interfaceTypes[index];
                if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition && interfaceType.FullName == null)
                {
                    interfaceTypes[index] = interfaceType.GetGenericTypeDefinition();
                }
            }
            return interfaceTypes;
        }

        /// <summary>
        /// 添加单例服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="descriptor"></param>
        /// <param name="dependencyAttribute"></param>
        private static void AddSingleService(IServiceCollection services, ServiceDescriptor descriptor, DependencyAttribute dependencyAttribute)
        {
            if (dependencyAttribute?.ReplaceServices == true)
            {
                services.Replace(descriptor);
            }
            else if (dependencyAttribute?.TryRegister == true)
            {
                services.TryAdd(descriptor);
            }
            else
            {
                services.Add(descriptor);
            }
        }

    }
}
