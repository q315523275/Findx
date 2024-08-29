using System.Net.NetworkInformation;
using System.Net.Sockets;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     DNS工具类
/// </summary>
public static class HostUtility
{
    /// <summary>
    ///     解析主机地址
    /// </summary>
    /// <param name="hostName"></param>
    /// <returns></returns>
    public static string ResolveHostAddress(string hostName)
    {
        try
        {
            if (CommonUtility.IsLinux)
                return NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Select(p => p.GetIPProperties())
                    .SelectMany(p => p.UnicastAddresses)
                    .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))
                    ?.Address.ToString();

            return Dns.GetHostAddresses(hostName)
                .FirstOrDefault(ip => ip.AddressFamily.Equals(AddressFamily.InterNetwork))?.ToString();
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    ///     解析主机Ip地址
    /// </summary>
    /// <param name="hostName"></param>
    /// <returns></returns>
    public static IPAddress ResolveHostIpAddress(string hostName)
    {
        try
        {
            if (CommonUtility.IsLinux)
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Select(p => p.GetIPProperties()).SelectMany(p => p.UnicastAddresses)
                    .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))
                    ?.Address;

            return Dns.GetHostAddresses(hostName)
                .FirstOrDefault(ip => ip.AddressFamily.Equals(AddressFamily.InterNetwork));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析主机名
    /// </summary>
    /// <returns></returns>
    public static string ResolveHostName()
    {
        string result = null;
        try
        {
            result = Dns.GetHostName();
            if (result.IsNotNullOrWhiteSpace()) return Dns.GetHostEntry(result).HostName;
        }
        catch
        {
            // ignored
        }

        return result;
    }
}