namespace Findx.Net.IP;

/// <summary>
///     IP地址位置定位接口
/// </summary>
public interface IIpGeolocation
{
    /// <summary>
    ///     根据IP地址获取地理位置信息
    /// </summary>
    /// <param name="ipAddress">ipAddress 待查询的IP地址（支持IPv4/IPv6）</param>
    /// <returns>包含国家/城市/经纬度/运营商等信息的文本</returns>
    string GetLocationTextByIp(string ipAddress);
    
    /// <summary>
    ///     批量查询IP地址的地理位置信息
    /// </summary>
    /// <param name="ipAddresses">ipAddresses IP地址列表</param>
    /// <returns>地理位置信息映射表</returns>
    Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses);
    
    /// <summary>
    ///     根据IP地址获取地理位置信息
    /// </summary>
    /// <param name="ipAddress">ipAddress 待查询的IP地址（支持IPv4/IPv6）</param>
    /// <returns>包含国家/城市/经纬度/运营商等信息的IpGeolocationVo对象</returns>
    IpGeolocationVo GetLocationByIp(string ipAddress);
    
    /// <summary>
    ///     批量查询IP地址的地理位置信息
    /// </summary>
    /// <param name="ipAddresses">ipAddresses IP地址列表</param>
    /// <returns>地理位置信息映射表</returns>
    Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses);
}