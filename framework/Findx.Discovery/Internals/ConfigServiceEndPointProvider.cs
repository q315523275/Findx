using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;
using Findx.Setting;

namespace Findx.Discovery.Internals;

/// <summary>
///     配置服务实例提供器
/// </summary>
public class ConfigServiceEndPointProvider: IServiceEndPointProvider, IServiceNameAware
{
    private readonly ISettingProvider _settingProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="settingProvider"></param>
    public ConfigServiceEndPointProvider(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    /// <summary>
    ///     提供器名称
    /// </summary>
    public string Name => "Config";

    /// <summary>
    ///     获取服务实例集合
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="group"></param>
    /// <param name="passingOnly"></param>
    /// <param name="tag"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<IServiceEndPoint>> GetServiceEndPointsAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var res = new List<ConfigServiceEndPoint>();
        var serviceEndPointDict = _settingProvider.GetObject<Dictionary<string, List<string>>>("ServiceEndPoint");
        if (serviceEndPointDict.TryGetValue(serviceName, out var endPoints))
        {
            res.AddRange(endPoints.Select(endPoint => new Uri(endPoint)).Select(uri => new ConfigServiceEndPoint { ServiceName = serviceName, Host = uri.Host, Port = uri.Port }));
        }
        return res;
    }

    /// <summary>
    ///     获取服务名称集合
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default)
    {
        var serviceEndPointDict = _settingProvider.GetObject<Dictionary<string, List<string>>>("ServiceEndPoint");
        return Task.FromResult(serviceEndPointDict.Keys.AsEnumerable());
    }
}