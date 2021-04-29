﻿using Findx.AspNetCore.Mvc;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using Findx.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;
using System.Security.Principal;

namespace Findx.AspNetCore
{
    [Description("Findx-AspNetCore模块")]
    public class AspNetCoreModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 10;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(provider =>
            {
                IHttpContextAccessor accessor = provider.GetService<IHttpContextAccessor>();
                return accessor?.HttpContext?.User;
            });

            services.TryAddSingleton<IScopedServiceResolver, HttpContextServiceScopeResolver>();
            services.Replace<ICancellationTokenProvider, HttpContextCancellationTokenProvider>(ServiceLifetime.Singleton);
            services.Replace<IHybridServiceScopeFactory, HttpContextServiceScopeFactory>(ServiceLifetime.Singleton);

            services.AddSingleton<IApiInterfaceService, DefaultApiInterfaceService>();

            // 关闭模型自动化验证,实现自控
            services.Configure<ApiBehaviorOptions>(opts => opts.SuppressModelStateInvalidFilter = true);
            // 让用户自控
            //services.AddControllersWithViews()
            //        .AddJsonOptions(options =>
            //        {
            //            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            //        });

            return services;
        }
    }
}
