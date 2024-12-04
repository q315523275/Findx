using System.ComponentModel;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.RulesEngine;

/// <summary>
///     Findx-RulesEngine模块
/// </summary>
[Description("Findx-RulesEngine模块")]
public class RulesEngineModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 160;
        
    /// <summary>
    ///     配置模块服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRulesEngineClient, RulesEngineClient>();
        return services;
    }
}