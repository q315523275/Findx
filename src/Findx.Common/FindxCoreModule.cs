using Findx.Caching;
using Findx.Caching.InMemory;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Email;
using Findx.ExceptionHandling;
using Findx.Extensions;
using Findx.Locks;
using Findx.Mapping;
using Findx.Messaging;
using Findx.Modularity;
using Findx.Reflection;
using Findx.Serialization;
using Findx.Sms;
using Findx.Storage;
using Findx.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace Findx.Builders
{
    [Description("Findx-基础模块")]
    public class FindxCoreModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 0;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = services.GetConfiguration();

            // 缓存
            services.TryAddSingleton<ICacheProvider, CacheProvider>();
            services.TryAddSingleton<ICacheKeyGenerator, StringCacheKeyGenerator>();
            services.AddSingleton<ICache, InMemoryCache>();

            // 注入模块
            services.TryAddSingleton<IHybridServiceScopeFactory, DefaultServiceScopeFactory>();
            services.AddScoped<ScopedDictionary>();

            // 工作单元
            services.AddScoped<IUnitOfWorkManager, NullUnitOfWorkManager>();

            // 邮件
            services.AddSingleton<IEmailSender, DefaultEmailSender>();
            services.Configure<EmailSenderOptions>(configuration.GetSection("Findx:Email"));

            // 映射配置
            services.Configure<MappingOptions>(configuration.GetSection("Findx:Mapping"));

            // 异常通知
            services.AddSingleton<IExceptionNotifier, ExceptionNotifier>();
            services.AddSingleton<IExceptionSubscriber, NullExceptionSubscriber>();

            // 锁
            services.AddSingleton<ILock, LocalLock>();

            // 反射查询器
            services.AddSingleton<IMethodInfoFinder, PublicInstanceMethodInfoFinder>();

            // 进程消息
            services.AddScoped<IMessageSender, DefaultMessageSender>();
            services.AddSingleton<IMessageNotifySender, DefaultMessageNotifySender>();

            // 序列化
            services.AddSingleton<IJsonSerializer, SystemTextJsonStringSerializer>();
            services.AddSingleton<ISerializer, SystemTextUtf8ByteSerializer>();

            // 短信
            services.AddSingleton<ISmsSender, NullSmsSender>();

            // 存储
            services.AddSingleton<IStorage, LocalStorage>();
            services.AddSingleton<IStorageProvider, StorageProvider>();

            // 线程取消通知
            services.AddSingleton<ICancellationTokenProvider, NullCancellationTokenProvider>();

            // 应用
            services.AddSingleton<IApplicationInstanceInfo, ApplicationInstanceInfo>();

            return services;
        }
    }
}
