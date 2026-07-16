using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Findx.Configuration;

/// <summary>
///     Http请求
/// </summary>
internal static class HttpUtil
{
    private static readonly SocketsHttpHandler HttpHandler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
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