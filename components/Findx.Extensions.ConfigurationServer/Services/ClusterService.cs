using System.Net.Http.Json;
using Findx.Extensions.ConfigurationServer.Dtos;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.ConfigurationServer.Services;

/// <summary>
///     集群服务
/// </summary>
public class ClusterService : IClusterService
{
    private const int MaxAttempts = 3;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<ConfigServerOptions> _options;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ClusterService(IHttpClientFactory httpClientFactory, IOptions<ConfigServerOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    /// <summary>
    ///     配置变更集群同步通知（指数退避重试，最多 3 次）
    /// </summary>
    public async Task ConfigChangeClusterSyncNotifyAsync(string nodeInfo, ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default)
    {
        var scheme = _options.Value.ClusterScheme ?? "http";
        var address = nodeInfo?.TrimEnd('/');
        var httpUrl = $"{scheme}://{address}/api/config/cluster/change-notify";

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, httpUrl);
                request.Content = JsonContent.Create(changeDto);
                if (_options.Value.ClusterToken.IsNotNullOrWhiteSpace())
                {
                    request.Headers.TryAddWithoutValidation("X-Cluster-Token", _options.Value.ClusterToken);
                }
                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception) when (attempt < MaxAttempts && !cancellationToken.IsCancellationRequested)
            {
                // 指数退避：1s / 2s，最后一次失败直接向上抛出交由调用方决策
                await Task.Delay(TimeSpan.FromMilliseconds(1000 * Math.Pow(2, attempt - 1)), cancellationToken);
            }
        }
    }
}
