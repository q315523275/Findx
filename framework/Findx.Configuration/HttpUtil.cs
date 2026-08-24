using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Findx.Configuration;

/// <summary>
///     Http工具
/// </summary>
internal static class HttpUtil
{
    private static readonly SocketsHttpHandler HttpHandler = new()
    {
        //  定期刷新 DNS，解决 K8s/容器环境下服务迁移导致的连接报错
        PooledConnectionLifetime = TimeSpan.FromMinutes(2), 
        //  保持连接活跃，减少握手开销
        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
    };
    
    private static readonly HttpClient HttpClient = new(HttpHandler);

    /// <summary>
    ///     异步方式发起HttpGet请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers, int? timeout)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers != null)
        {
            foreach (var kvp in headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
        }

        if (!timeout.HasValue) return await HttpClient.SendAsync(request);
        
        using var cts = new System.Threading.CancellationTokenSource();
        cts.CancelAfter(timeout.Value);
        return await HttpClient.SendAsync(request, cts.Token);
    }
}