using System.Net.NetworkInformation;

namespace Findx.Utils;

/// <summary>
///     网络辅助工具类
/// </summary>
public static class NetUtil
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
}