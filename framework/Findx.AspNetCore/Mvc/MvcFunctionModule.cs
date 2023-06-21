using System.ComponentModel;
using Findx.Modularity;
using Findx.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     Findx-Mvc功能信息模块
/// </summary>
[Description("Findx-Mvc功能信息模块")]
public class MvcFunctionModule : StartupModule
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 14;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFunctionHandler, MvcFunctionHandler>();
        services.AddSingleton<IFunctionStore<MvcFunction>, MvcFunctionStore>();

        services.AddHostedService<MvcFunctionWorker>();

        return services;
    }
}