using System.ComponentModel;
using Findx.Modularity;
using Findx.Module.ConfigService.Client;
using Findx.Module.ConfigService.Services;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Findx.Messaging;
using Findx.Module.ConfigService.Handling;

namespace Findx.Module.ConfigService;

/// <summary>
/// 配置服务模块
/// </summary>
[Description("Findx配置中心-服务端模块")]
public class ConfigServiceModule: FindxModule
{
    public override ModuleLevel Level => ModuleLevel.Application;

    public override int Order => 110;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IClientCallBack, ClientCallBack>();
        services.AddSingleton<IClusterService, ClusterService>();
        services.AddSingleton<IDumpService, DumpService>();
        services.AddScoped<IApplicationEventHandler<ConfigDataChangeEvent>, ConfigDataChangeEventHandler>();

        var configuration = services.GetConfiguration();

        // 应用基础
        services.Configure<ConfigServiceOptions>(configuration.GetSection("Findx:ConfigService"));

        return base.ConfigureServices(services);
    }
}