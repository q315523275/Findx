﻿using Findx.Builders;
using Findx.Logging;
using Findx.Modularity;
using Findx.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 服务
    /// </summary>
    public static partial class Extensions
    {
        #region IServiceCollection
        /// <summary>
        /// 添加Findx服务构建
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IFindxBuilder AddFindx(this IServiceCollection services)
        {
            // 框架启动异步日志
            services.GetOrAddSingletonInstance(() => new StartupLogger());
            services.GetOrAddSingletonInstance<IAppDomainAssemblyFinder>(() => new AppDomainAssemblyFinder());
            // 框架构建
            IFindxBuilder builder = services.GetOrAddSingletonInstance<IFindxBuilder>(() => new FindxBuilder(services));
            return builder;
        }

        /// <summary>
        /// 获取<see cref="IConfiguration"/>配置信息
        /// </summary>
        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            return services.GetSingletonInstanceOrNull<IConfiguration>();
        }

        /// <summary>
        /// 判断指定服务类型是否存在
        /// </summary>
        public static bool AnyServiceType(this IServiceCollection services, Type serviceType)
        {
            return services.Any(m => m.ServiceType == serviceType);
        }

        /// <summary>
        /// 替换服务
        /// </summary>
        public static IServiceCollection Replace<TService, TImplement>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplement), lifetime);
            services.Replace(descriptor);
            return services;
        }

        /// <summary>
        /// 获取服务描述，不存在则添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="toAdDescriptor"></param>
        /// <returns></returns>
        public static ServiceDescriptor GetOrAdd(this IServiceCollection services, ServiceDescriptor toAdDescriptor)
        {
            ServiceDescriptor descriptor = services.FirstOrDefault(m => m.ServiceType == toAdDescriptor.ServiceType);
            if (descriptor != null)
            {
                return descriptor;
            }

            services.Add(toAdDescriptor);
            return toAdDescriptor;
        }

        /// <summary>
        /// 获取或添加指定类型查找器
        /// </summary>
        public static TTypeFinder GetOrAddTypeFinder<TTypeFinder>(this IServiceCollection services, Func<IAppDomainAssemblyFinder, TTypeFinder> factory)
            where TTypeFinder : class
        {
            return services.GetOrAddSingletonInstance(() =>
            {
                IAppDomainAssemblyFinder allAssemblyFinder =
                    services.GetOrAddSingletonInstance<IAppDomainAssemblyFinder>(() => new AppDomainAssemblyFinder(true));
                return factory(allAssemblyFinder);
            });
        }

        /// <summary>
        /// 如果指定服务不存在，创建实例并添加
        /// </summary>
        public static TServiceType GetOrAddSingletonInstance<TServiceType>(this IServiceCollection services, Func<TServiceType> factory) where TServiceType : class
        {
            TServiceType item = GetSingletonInstanceOrNull<TServiceType>(services);
            if (item == null)
            {
                item = factory();
                services.AddSingleton(item);
            }
            return item;
        }

        /// <summary>
        /// 获取单例注册服务对象
        /// </summary>
        public static T GetSingletonInstanceOrNull<T>(this IServiceCollection services)
        {
            ServiceDescriptor descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

            if (descriptor?.ImplementationInstance != null)
            {
                return (T)descriptor.ImplementationInstance;
            }

            if (descriptor?.ImplementationFactory != null)
            {
                return (T)descriptor.ImplementationFactory.Invoke(null);
            }

            return default;
        }

        /// <summary>
        /// 获取单例注册服务对象
        /// </summary>
        public static T GetSingletonInstance<T>(this IServiceCollection services)
        {
            var instance = services.GetSingletonInstanceOrNull<T>();
            if (instance == null)
            {
                throw new InvalidOperationException($"无法找到已注册的单例服务：{typeof(T).AssemblyQualifiedName}");
            }

            return instance;
        }

        /// <summary>
        /// 通过工厂进行服务器提供器构建
        /// </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <typeparam name="TContainerBuilder"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IServiceProvider BuildServiceProviderFromFactory<TContainerBuilder>([NotNull] this IServiceCollection services, Action<TContainerBuilder> builderAction = null)
        {
            Check.NotNull(services, nameof(services));

            var serviceProviderFactory = services.GetSingletonInstanceOrNull<IServiceProviderFactory<TContainerBuilder>>();
            if (serviceProviderFactory == null)
            {
                throw new Exception($"Could not find {typeof(IServiceProviderFactory<TContainerBuilder>).FullName} in {services}.");
            }

            var builder = serviceProviderFactory.CreateBuilder(services);
            builderAction?.Invoke(builder);
            return serviceProviderFactory.CreateServiceProvider(builder);
        }

        #endregion

        #region IServiceProvider
        /// <summary>
        /// 获取所有模块信息
        /// </summary>
        public static IEnumerable<FindxModule> GetAllModules(this IServiceProvider provider)
        {
            return provider.GetServices<FindxModule>().OrderBy(m => m.Level).ThenBy(m => m.Order).ThenBy(m => m.GetType().FullName);
        }

        /// <summary>
        /// 获取当前用户
        /// </summary>
        public static ClaimsPrincipal GetCurrentUser(this IServiceProvider provider)
        {
            try
            {
                var user = provider.GetService<IPrincipal>();
                return user as ClaimsPrincipal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 框架初始化，适用于非AspNetCore环境
        /// </summary>
        public static IServiceProvider UseFindx(this IServiceProvider provider)
        {
            var logger = provider.GetLogger(typeof(Extensions));
            logger.LogInformation("框架初始化开始");
            var watch = Stopwatch.StartNew();

            var modules = provider.GetServices<FindxModule>();
            foreach (var module in modules)
            {
                var jsTime = DateTime.Now;
                var moduleType = module.GetType();
                module.UseModule(provider);
                logger.LogInformation($"模块《{moduleType.GetDescription()}》({moduleType.Name})” 初始化完成，耗时{(DateTime.Now - jsTime).TotalMilliseconds}ms");
            }

            watch.Stop();
            logger.LogInformation($"框架初始化完毕，耗时：{watch.Elapsed.TotalMilliseconds}毫秒");

            return provider;
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <typeparam name="T">非静态强类型</typeparam>
        /// <returns>日志对象</returns>
        public static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            var factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger<T>();
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type">指定类型</param>
        /// <returns>日志对象</returns>
        public static ILogger GetLogger(this IServiceProvider provider, Type type)
        {
            Check.NotNull(type, nameof(type));
            var factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(type);
        }

        /// <summary>
        /// 获取指定对象类型的日志对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="instance">要获取日志的类型对象，一般指当前类，即this</param>
        public static ILogger GetLogger(this IServiceProvider provider, object instance)
        {
            Check.NotNull(instance, nameof(instance));
            var factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(instance.GetType());
        }

        /// <summary>
        /// 获取指定名称的日志对象
        /// </summary>
        public static ILogger GetLogger(this IServiceProvider provider, string name)
        {
            var factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(name);
        }

        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑
        /// </summary>
        public static void ExecuteScopedWork(this IServiceProvider provider, Action<IServiceProvider> action)
        {
            using var scope = provider.CreateScope();
            action(scope.ServiceProvider);
        }

        /// <summary>
        /// 异步执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑
        /// </summary>
        public static async Task ExecuteScopedWorkAsync(this IServiceProvider provider, Func<IServiceProvider, Task> action)
        {
            using var scope = provider.CreateScope();
            await action(scope.ServiceProvider);
        }

        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑，并获取返回值
        /// </summary>
        public static TResult ExecuteScopedWork<TResult>(this IServiceProvider provider, Func<IServiceProvider, TResult> func)
        {
            using var scope = provider.CreateScope();
            return func(scope.ServiceProvider);
        }

        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑，并获取返回值
        /// </summary>
        public static async Task<TResult> ExecuteScopedWorkAsync<TResult>(this IServiceProvider provider, Func<IServiceProvider, Task<TResult>> func)
        {
            using var scope = provider.CreateScope();
            return await func(scope.ServiceProvider);
        }
        #endregion

        #region Log
        /// <summary>
        /// 添加启动调试日志
        /// </summary>
        public static IServiceCollection LogDebug(this IServiceCollection services, string message, string logName)
        {
            var logger = services.GetOrAddSingletonInstance(() => new StartupLogger());
            logger.LogDebug(message, logName);
            return services;
        }

        /// <summary>
        /// 添加启动消息日志
        /// </summary>
        public static IServiceCollection LogInformation(this IServiceCollection services, string message, string logName)
        {
            var logger = services.GetOrAddSingletonInstance(() => new StartupLogger());
            logger.LogInformation(message, logName);
            return services;
        }
        #endregion
    }
}
