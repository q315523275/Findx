using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Findx.Utilities;

namespace Findx.Machine.Network;

/// <summary>
///     网络接口信息
/// </summary>
public class NetworkInfo
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="network"></param>
    private NetworkInfo(NetworkInterface network)
    {
        NetworkInterface = network;
    }

    /// <summary>
    ///     当前实例使用的网络接口
    /// </summary>
    public NetworkInterface NetworkInterface { get; }

    /// <summary>
    ///     当前主机是否能够与其他计算机通讯(公网或内网)，如果任何网络接口标记为 "up" 且不是环回或隧道接口，则认为网络连接可用。
    /// </summary>
    public static bool GetIsNetworkAvailable => NetworkInterface.GetIsNetworkAvailable();

    /// <summary>
    ///     计算 IPV4 的网络流量
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">当前网卡不支持 IPV4</exception>
    public Rate GetIpv4Speed()
    {
        // 当前网卡不支持 IPV4
        if (!IsSupportIpv4) return default;
        var ipv4Statistics = NetworkInterface.GetIPv4Statistics();
        var speed = new Rate(DateTime.Now, ipv4Statistics.BytesReceived, ipv4Statistics.BytesSent);
        return speed;
    }

    /// <summary>
    ///     计算 IPV4 、IPV6 的网络流量
    /// </summary>
    /// <returns></returns>
    public Rate IpvSpeed()
    {
        var ipvStatistics = NetworkInterface.GetIPStatistics();
        var speed = new Rate(DateTime.Now, ipvStatistics.BytesReceived, ipvStatistics.BytesSent);
        return speed;
    }

    /// <summary>
    ///     获取所有 IP 地址
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<IPAddress> GetIpAddresses()
    {
        var hostName = Dns.GetHostName();
        return Dns.GetHostAddresses(hostName);
    }

    /// <summary>
    ///     获取当前真实 IP
    /// </summary>
    /// <returns></returns>
    public static IPAddress TryGetRealIpv4()
    {
        if (CommonUtility.IsLinux)
            return NetworkInterface.GetAllNetworkInterfaces().Select(p => p.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses)
                .FirstOrDefault(p =>
                    p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))
                ?.Address;

        var address = GetIpAddresses();
        var ipv4 = address.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        return ipv4;
    }

    /// <summary>
    ///     获取真实网卡
    /// </summary>
    /// <returns></returns>
    public static NetworkInfo TryGetRealNetworkInfo()
    {
        var realIp = TryGetRealIpv4();
        if (realIp == null) return default;

        return GetNetworkInfos().FirstOrDefault(x =>
            x.UnicastAddresses.Any(address => address.MapToIPv4().ToString() == realIp.MapToIPv4().ToString()));
    }

    /// <summary>
    ///     获取此主机中所有网卡接口
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<NetworkInfo> GetNetworkInfos()
    {
        return NetworkInterface.GetAllNetworkInterfaces().Select(x => new NetworkInfo(x));
    }

    /// <summary>
    ///     计算网络流量速率
    /// </summary>
    /// <param name="oldRate"></param>
    /// <param name="newRate"></param>
    /// <returns></returns>
    public static (SizeInfo Received, SizeInfo Sent) GetSpeed(Rate oldRate, Rate newRate)
    {
        var receive = newRate.ReceivedLength - oldRate.ReceivedLength;
        var send = newRate.SendLength - oldRate.SendLength;
        var interval = Math.Round((newRate.StartTime - oldRate.StartTime).TotalSeconds, 2);

        var rSpeed = (long)(receive / interval);
        var sSpeed = (long)(send / interval);

        return (SizeInfo.Get(rSpeed), SizeInfo.Get(sSpeed));
    }


    #region 基础信息

    /// <summary>
    ///     获取网络适配器的标识符
    /// </summary>
    /// <remarks>ex：{92D3E528-5363-43C7-82E8-D143DC6617ED}</remarks>
    public string Id => NetworkInterface.Id;

    /// <summary>
    ///     网络的 Mac 地址
    /// </summary>
    /// <remarks>ex： 1C997AF108E3</remarks>
    public string Mac => NetworkInterface.GetPhysicalAddress().ToString();

    /// <summary>
    ///     网卡名称
    /// </summary>
    /// <remarks>ex：以太网，WLAN</remarks>
    public string Name => NetworkInterface.Name;

    /// <summary>
    ///     描述网络接口的用户可读文本，
    ///     在 Windows 上，它通常描述接口供应商、类型 (例如，以太网) 、品牌和型号；
    /// </summary>
    /// <remarks>ex：Realtek PCIe GbE Family Controller、  Realtek 8822CE Wireless LAN 802.11ac PCI-E NIC</remarks>
    public string Trademark => NetworkInterface.Description;

    /// <summary>
    ///     获取网络连接的当前操作状态<br />
    /// </summary>
    public OperationalStatus Status => NetworkInterface.OperationalStatus;

    /// <summary>
    ///     获取网卡接口类型<br />
    /// </summary>
    public NetworkInterfaceType NetworkType => NetworkInterface.NetworkInterfaceType;

    /// <summary>
    ///     网卡链接速度，每字节/秒为单位
    /// </summary>
    /// <remarks>如果是-1，则说明无法获取此网卡的链接速度；例如 270_000_000 表示是 270MB 的链接速度</remarks>
    public long Speed => NetworkInterface.Speed;

    /// <summary>
    ///     是否支持 Ipv4
    /// </summary>
    public bool IsSupportIpv4 => NetworkInterface.Supports(NetworkInterfaceComponent.IPv4);

    /// <summary>
    ///     获取分配给此接口的任意广播 IP 地址。只支持 Windows
    /// </summary>
    /// <remarks>一般情况下为空数组</remarks>
    public IEnumerable<IPAddress> AnycastAddresses
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return NetworkInterface.GetIPProperties().AnycastAddresses.Select(x => x.Address);

            return Array.Empty<IPAddress>();
        }
    }

    /// <summary>
    ///     获取分配给此接口的多播地址，ipv4、ipv6
    /// </summary>
    /// <remarks>
    ///     ex：ff01::1%9 ff02::1%9<br />
    ///     ff02::fb%9<br />
    ///     ff02::1:3%9<br />
    ///     ff02::1:ff61:9ae7%9<br />
    ///     224.0.0.1
    /// </remarks>
    public IEnumerable<IPAddress> MulticastAddresses =>
        NetworkInterface.GetIPProperties().MulticastAddresses.Select(x => x.Address);

    /// <summary>
    ///     获取分配给此接口的单播地址，ipv4、ipv6
    /// </summary>
    /// <remarks>ex：192.168.3.38</remarks>
    public IEnumerable<IPAddress> UnicastAddresses =>
        NetworkInterface.GetIPProperties().UnicastAddresses.Select(x => x.Address);

    /// <summary>
    ///     获取此接口的 IPv4 网关地址，ipv4、ipv6
    /// </summary>
    /// <remarks>ex：fe80::1677:40ff:fef9:bf95%5、192.168.3.1</remarks>
    public IEnumerable<IPAddress> GatewayAddresses =>
        NetworkInterface.GetIPProperties().GatewayAddresses.Select(x => x.Address);

    /// <summary>
    ///     获取此接口的域名系统 (DNS) 服务器的地址，ipv4、ipv6
    /// </summary>
    /// <remarks>ex：fe80::1677:40ff:fef9:bf95%5、192.168.3.1</remarks>
    public IPAddress[] DnsAddresses => NetworkInterface.GetIPProperties().DnsAddresses.ToArray();

    /// <summary>
    ///     是否支持 Ipv6
    /// </summary>
    public bool IsSupportIpv6 => NetworkInterface.Supports(NetworkInterfaceComponent.IPv6);

    #endregion
}