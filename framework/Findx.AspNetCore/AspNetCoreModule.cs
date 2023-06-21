using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Upload;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using Findx.Security;
using Findx.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.AspNetCore;

/// <summary>
///     Findx-AspNetCore模块
/// </summary>
[Description("Findx-AspNetCore模块")]
public class AspNetCoreModule : StartupModule
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 10;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddHttpContextAccessor();
        services.TryAddTransient<IPrincipal>(provider =>
            provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);
        services.TryAddSingleton<ICurrentUser, CurrentUser>();

        services.TryAddSingleton<IScopedServiceResolver, HttpContextServiceScopeResolver>();

        services.Replace<ICancellationTokenProvider, HttpContextCancellationTokenProvider>(ServiceLifetime.Singleton);
        services.Replace<IHybridServiceScopeFactory, HttpContextServiceScopeFactory>(ServiceLifetime.Singleton);

        services.AddSingleton<IFileUploadService, DefaultFileUploadService>();

        // 关闭模型自动化验证,实现自控
        services.Configure<ApiBehaviorOptions>(opts => opts.SuppressModelStateInvalidFilter = true);

        return services;
    }
}