using Findx.Extensions.ConfigurationServer.Dtos;

namespace Findx.Extensions.ConfigurationServer.Services;

/// <summary>
///     集群服务
/// </summary>
public class ClusterService : IClusterService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="httpClientFactory"></param>
    public ClusterService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    ///     配置变更集群同步通知
    /// </summary>
    /// <param name="nodeInfo"></param>
    /// <param name="changeDto"></param>
    /// <param name="cancellationToken"></param>
    public async Task ConfigChangeClusterSyncNotifyAsync(string nodeInfo, ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        var httpUrl = $"http://{nodeInfo}/api/config/cluster/configChangeNotify";
        var response = await client.PostAsJsonAsync(httpUrl, changeDto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}