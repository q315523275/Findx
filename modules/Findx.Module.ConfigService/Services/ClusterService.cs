using Findx.Extensions;
using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Services;

public class ClusterService: IClusterService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ClusterService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task ConfigChangeClusterSyncNotifyAsync(string nodeInfo, ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        var httpUrl = $"http://{nodeInfo}/api/config/cluster/configChangeNotify";
        var response = await client.PostAsJsonAsync(httpUrl, changeDto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}