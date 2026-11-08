namespace Findx.Net.IP;

/// <summary>
///     IP地理定位占位实现（未接入真实 IP 库时的默认实现，安全返回"未知"而非抛异常）
/// </summary>
public class NullIpGeolocation : IIpGeolocation
{
    private readonly ILogger<NullIpGeolocation> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public NullIpGeolocation(ILogger<NullIpGeolocation> logger)
    {
        _logger = logger;
    }

    public string GetLocationTextByIp(string ipAddress)
    {
        _logger.LogDebug("IP地理定位未配置实现，返回未知位置: {IpAddress}", ipAddress);
        return null;
    }

    public Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses)
    {
        _logger.LogDebug("IP地理定位未配置实现，批量返回空结果");
        return new Dictionary<string, string>();
    }

    public IpGeolocationVo GetLocationByIp(string ipAddress)
    {
        _logger.LogDebug("IP地理定位未配置实现，返回未知位置: {IpAddress}", ipAddress);
        return null;
    }

    public Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses)
    {
        _logger.LogDebug("IP地理定位未配置实现，批量返回空结果");
        return new Dictionary<string, IpGeolocationVo>();
    }
}