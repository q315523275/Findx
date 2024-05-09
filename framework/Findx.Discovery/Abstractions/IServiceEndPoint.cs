using System;
using System.Collections.Generic;
using System.Net;

namespace Findx.Discovery.Abstractions;

/// <summary>
///     服务实例
/// </summary>
public interface IServiceEndPoint
{
    /// <summary>
    ///     服务名
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    ///     Host
    /// </summary>
    string Host { get; }

    /// <summary>
    ///     端口
    /// </summary>
    int Port { get; }

    /// <summary>
    ///     元数据
    /// </summary>
    IDictionary<string, string> Metadata { get; }
}

/// <summary>
///     服务实例扩展
/// </summary>
public static class ServiceInstanceExtension
{
    /// <summary>
    ///     转换为Http Uri
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="scheme"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Uri ToUri(this IServiceEndPoint serviceEndPoint, string scheme = "http", string path = "/")
    {
        return new UriBuilder(scheme, serviceEndPoint.Host, serviceEndPoint.Port, path).Uri;
    }

    /// <summary>
    ///     转换为IP终结点
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <returns></returns>
    public static EndPoint ToIpEndPoint(this IServiceEndPoint serviceEndPoint)
    {
        return new IPEndPoint(IPAddress.Parse(serviceEndPoint.Host), serviceEndPoint.Port);
    }
}