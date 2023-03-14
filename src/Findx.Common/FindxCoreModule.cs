using System.ComponentModel;
using Findx.Caching;
using Findx.Caching.InMemory;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Domain;
using Findx.Email;
using Findx.ExceptionHandling;
using Findx.Extensions;
using Findx.Guids;
using Findx.Locks;
using Findx.Mapping;
using Findx.Messaging;
using Findx.Modularity;
using Findx.Reflection;
using Findx.Security;
using Findx.Serialization;
using Findx.Setting;
using Findx.Sms;
using Findx.Storage;
using Findx.Threading;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx
{
    /// <summary>
    /// Findx-基础模块
    /// </summary>
    [Description("Findx-基础模块")]
    public class FindxCoreModule : FindxModule
    {
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;
        
        /// <summary>
        /// 排序
        /// 模块启动顺序，模块启动的顺序先按级别启动，同一级别内部再按此顺序启动，
        /// </summary>
        public override int Order => 0;
        
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            var configuration = services.GetConfiguration();
            
            // 应用基础
            services.Configure<ApplicationOptions>(configuration.GetSection("Findx:Application"));

            // 配置
            services.AddSingleton<ISettingProvider, ConfigurationSettingProvider>();

            // 缓存
            services.TryAddSingleton<ICacheProvider, CacheProvider>();
            services.TryAddSingleton<ICacheKeyGenerator, StringCacheKeyGenerator>();
            services.AddSingleton<ICache, InMemoryCache>();

            // 注入模块
            services.TryAddSingleton<IHybridServiceScopeFactory, DefaultServiceScopeFactory>();
            services.AddScoped<ScopedDictionary>();

            // 工作单元
            services.AddScoped<IUnitOfWorkManager, NullUnitOfWorkManager>();
            
            // Entity、Domain
            // services.TryAddSingleton<IEntityFinder, EntityFinder>();
            // services.TryAddSingleton<IDomainEventsDispatcher, DomainEventsDispatcher>();

            // 邮件
            services.AddSingleton<IEmailSender, DefaultEmailSender>();
            services.Configure<EmailSenderOptions>(configuration.GetSection("Findx:Email"));

            // 映射配置
            services.Configure<MappingOptions>(configuration.GetSection("Findx:Mapping"));

            // 异常通知
            services.AddSingleton<IExceptionNotifier, ExceptionNotifier>();
            services.AddSingleton<IExceptionSubscriber, NullExceptionSubscriber>();

            // 锁
            services.AddSingleton<ILock, LocalCacheLock>();
            services.AddSingleton<ILockProvider, LockProvider>();

            // 反射查询器
            services.AddSingleton<IMethodInfoFinder, PublicInstanceMethodInfoFinder>();

            // 进程消息
            services.AddScoped<IMessageDispatcher, MessageDispatcher>();
            services.AddSingleton<IApplicationEventPublisher, ApplicationEventPublisher>();

            // 序列化
            services.AddSingleton<IJsonSerializer, SystemTextJsonStringSerializer>();
            services.AddSingleton<ISerializer, SystemTextUtf8ByteSerializer>();

            // 短信
            services.AddSingleton<ISmsSender, NullSmsSender>();

            // 存储
            services.AddSingleton<IFileStorage, FolderFileStorage>();
            services.AddSingleton<IStorageProvider, StorageProvider>();

            // 线程取消通知
            services.AddSingleton<ICancellationTokenProvider, NullCancellationTokenProvider>();

            // 应用上下文
            services.AddSingleton<IApplicationContext, ApplicationContext>();

            // 有序Guid
            services.Configure<SequentialGuidOptions>(configuration.GetSection("Findx:SequentialGuid"));
            services.AddSingleton<IGuidGenerator, Guids.SequentialGuidGenerator>();

            // 主键生成器
            services.AddSingleton<IKeyGenerator<long>, SnowflakeIdGenerator>();
            services.AddSingleton<IKeyGenerator<Guid>, Data.SequentialGuidGenerator>();
            
            // 功能权限
            services.AddScoped<IFunctionAuthorization, FunctionAuthorization>();
            
            // 审计配置
            services.Configure<AuditingOptions>(configuration.GetSection("Findx:Auditing"));

            return services;
        }
    }
}
