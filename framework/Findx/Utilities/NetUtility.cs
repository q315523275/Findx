using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Findx.Utilities;

/// <summary>
///     网络辅助工具类
/// </summary>
public static class NetUtility
{
    /// <summary>
    ///     是否能Ping通指定主机
    /// </summary>
    public static bool Ping(string ip)
    {
        try
        {
            var ping = new Ping();
            var options = new PingOptions { DontFragment = true };
            const string data = "Test Data";
            var buffer = Encoding.ASCII.GetBytes(data);
            const int timeout = 1000;
            var reply = ping.Send(ip, timeout, buffer, options);
            return reply is { Status: IPStatus.Success };
        }
        catch (PingException)
        {
            return false;
        }
    }

    /// <summary>
    ///     是否内网IP
    /// </summary>
    /// <param name="ipv4Address"></param>
    /// <returns></returns>
    public static bool IsInternalIp(string ipv4Address)
    {
        if (!IPAddress.TryParse(ipv4Address, out var ip)) return false;
        
        var ipBytes = ip.GetAddressBytes();
        if (ipBytes[0] == 10) return true;
        if (ipBytes[0] == 127 && ipBytes[1] == 0) return true;
        if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true;
        if (ipBytes[0] == 192 && ipBytes[1] == 168) return true;

        return false;
    }

    /// <summary>
    ///     是否内网IP
    /// </summary>
    /// <param name="ipv4Address"></param>
    /// <returns></returns>
    public static bool IsInternalIp(IPAddress ipv4Address)
    {
        var ipBytes = ipv4Address.GetAddressBytes();
        
        if (ipBytes[0] == 10) return true;
        if (ipBytes[0] == 127 && ipBytes[1] == 0) return true;
        if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true;
        if (ipBytes[0] == 192 && ipBytes[1] == 168) return true;
        
        return false;
    }

    /// <summary>
    /// 判断url是否是外部地址
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static bool IsExternalAddress(this string url)
    {
        var uri = new Uri(url);
        switch (uri.HostNameType)
        {
            case UriHostNameType.Dns:
                var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                if (ipHostEntry.AddressList.Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork).Any(ipAddress => !IsInternalIp(ipAddress)))
                {
                    return true;
                }

                break;

            case UriHostNameType.IPv4:
                return !IsInternalIp(IPAddress.Parse(uri.DnsSafeHost));
            
            case UriHostNameType.Unknown:
                break;
            case UriHostNameType.Basic:
                break;
            case UriHostNameType.IPv6:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    
    /// <summary>
    ///     Ip地址转换成数字
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <returns>数字,输入无效IP地址返回0</returns>
    public static uint IpAddressToUInt32(string ipAddress)
    {
        return !IPAddress.TryParse(ipAddress, out var ip) ? 0 : IpAddressToUInt32(ip);
    }
    
    /// <summary>
    ///     Ip地址转换成数字
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>数字,输入无效IP地址返回0</returns>
    public static uint IpAddressToUInt32(IPAddress ipAddress)
    {
        var bInt = ipAddress.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bInt);
        }
        return BitConverter.ToUInt32(bInt, 0);
    }
}