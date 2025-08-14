using System;
using System.ComponentModel;
using System.IO;
using Findx.Modularity;
using Findx.Net.IP;
using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.IP;

/// <summary>
///     离线IP地址定位模块
/// </summary>
[Description("Findx-离线IP地址定位模块")]
public class IpGeolocationModule: StartupModule
{
    public override ModuleLevel Level => ModuleLevel.Application;

    public override int Order => 240;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISearcher>(new Searcher(CachePolicy.Content , Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip2region.xdb")));
        services.Replace(new ServiceDescriptor(typeof(IIpGeolocation), typeof(IpSearchServiceDefault), ServiceLifetime.Singleton));
        return services;
    }
}