namespace Findx.Net.IP;

/// <summary>
///     Ip地址定位信息
/// </summary>
public class IpGeolocationVo
{
    /// <summary>
    ///     原始查询的IP地址
    /// </summary>
    public string IpAddress { set; get; }
    
    /// <summary>
    ///     国家名称（如：中国）
    /// </summary>
    public string Country { set; get; }
    
    /// <summary>
    ///     国家ISO代码（如：CN）
    /// </summary>
    public string CountryCode { set; get; }
    
    /// <summary>
    ///     省
    /// </summary>
    public string Province { set; get; }
    
    /// <summary>
    ///     城市（如：深圳市）
    /// </summary>
    public string City { set; get; }
    
    /// <summary>
    ///     区/县（如：南山区，可选）
    /// </summary>
    public string District { set; get; }
    
    
    /// <summary>
    ///     经度（如：114.0579）
    /// </summary>
    public string Longitude { set; get; }
    
    /// <summary>
    ///     纬度（如：22.5431）
    /// </summary>
    public string Latitude { set; get; }
    
    /// <summary>
    ///     ISP（如：中国电信）
    /// </summary>
    public string Isp { set; get; }
    
    /// <summary>
    ///     是否内网IP
    /// </summary>
    public bool IsInternalIp { set; get; }
}