using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Configuration;

/// <summary>
///     Http请求
/// </summary>
internal static class HttpUtil
{
    /// <summary>
    ///     同步方式发起HttpGet请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public static HttpWebResponse Get(string url, IDictionary<string, string> headers, int? timeout)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);

        if (timeout.HasValue)
            request.Timeout = timeout.Value;

        if (headers == null) 
            return (HttpWebResponse)request.GetResponse();
            
        foreach (var kvp in headers)
            request.Headers.Add(kvp.Key, kvp.Value);

        return (HttpWebResponse)request.GetResponse();
    }

    /// <summary>
    ///     异步方式发起HttpGet请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public static async Task<HttpWebResponse> GetAsync(string url, Dictionary<string, string> headers, int? timeout)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);

        if (timeout.HasValue)
            request.Timeout = timeout.Value;

        if (headers != null)
            foreach (var kvp in headers)
                request.Headers.Add(kvp.Key, kvp.Value);

        var response = await request.GetResponseAsync();
        return (HttpWebResponse)response;
    }

    /// <summary>
    ///     获取Response内容
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task<string> ReadAsStringAsync(this HttpWebResponse response)
    {
        await using var responseStream = response.GetResponseStream();
        if (responseStream?.Length == 0) return string.Empty;
        using var reader = new StreamReader(responseStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();

    }

    /// <summary>
    ///     获取Response内容
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static string ReadAsString(this HttpWebResponse response)
    {
        using var responseStream = response.GetResponseStream();
        if (responseStream?.Length == 0) return string.Empty;
        using var reader = new StreamReader(responseStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}