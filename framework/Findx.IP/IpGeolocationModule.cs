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
    /// <summary>
    ///     ip2region.xdb 文件路径，可在 ConfigureServices 前由宿主自定义
    /// </summary>
    public static string XdbPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip2region.xdb");

    public override ModuleLevel Level => ModuleLevel.Application;

    public override int Order => 240;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        //  xdb 文件缺失时降级为空 Searcher，避免应用启动即崩溃
        if (File.Exists(XdbPath))
        {
            services.AddSingleton<ISearcher>(new Searcher(CachePolicy.Content, XdbPath));
        }
        else
        {
            services.AddSingleton<ISearcher>(new NullSearcher());
        }

        services.Replace(new ServiceDescriptor(typeof(IIpGeolocation), typeof(IpSearchServiceDefault), ServiceLifetime.Singleton));
        return services;
    }

    /// <summary>
    ///     xdb 缺失时的降级 Searcher，查询一律返回 null（即"未知"）
    /// </summary>
    private sealed class NullSearcher : ISearcher
    {
        public string Search(string ipAddress)
        {
            return null;
        }

        public string Search(uint ipAddress)
        {
            return null;
        }

        public string Search(System.Net.IPAddress ipAddress)
        {
            return null;
        }

        public int IoCount { get; set; }

        public void Dispose()
        {
        }
    }
}