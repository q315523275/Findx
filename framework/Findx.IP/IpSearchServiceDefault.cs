using System.Collections.Generic;
using Findx.Extensions;
using Findx.Net.IP;
using Findx.Utilities;
using IP2Region.Net.Abstractions;

namespace Findx.IP;

/// <summary>
///     IP归属查询默认实现
/// </summary>
public class IpSearchServiceDefault: IIpGeolocation
{
    private readonly ISearcher _searcher;

    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="searcher"></param>
    public IpSearchServiceDefault(ISearcher searcher)
    {
        _searcher = searcher;
    }

    /// <summary>
    ///     根据IP地址获取地理位置信息
    /// </summary>
    /// <param name="ipAddress">ipAddress 待查询的IP地址（支持IPv4/IPv6）</param>
    /// <returns>包含国家/城市/经纬度/运营商等信息的文本</returns>
    public string GetLocationTextByIp(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        if (NetUtility.IsInternalIp(ipAddress))
        {
            return "局域网";
        }

        return _searcher.Search(ipAddress);
    }

    /// <summary>
    ///     批量查询IP地址的地理位置信息
    /// </summary>
    /// <param name="ipAddresses">ipAddresses IP地址列表</param>
    /// <returns>地理位置信息映射表</returns>
    public Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses)
    {
        var dict = new Dictionary<string, string>();
        foreach (var item in ipAddresses)
        {
            //  跳过 null/空白项，避免 Dictionary 键异常
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            dict[item] = GetLocationTextByIp(item);
        }
        return dict;
    }

    /// <summary>
    ///     根据IP地址获取地理位置信息
    /// </summary>
    /// <param name="ipAddress">ipAddress 待查询的IP地址（支持IPv4/IPv6）</param>
    /// <returns>包含国家/城市/经纬度/运营商等信息的IpGeolocationVo对象</returns>
    public IpGeolocationVo GetLocationByIp(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return new IpGeolocationVo { IsInternalIp = false, IpAddress = ipAddress };
        }

        if (NetUtility.IsInternalIp(ipAddress))
        {
            return new IpGeolocationVo { IsInternalIp = true, IpAddress = ipAddress };
        }

        var geolocationStr = _searcher.Search(ipAddress);
        if (geolocationStr.IsNullOrWhiteSpace())
        {
            return new IpGeolocationVo { IsInternalIp = false, IpAddress = ipAddress };
        }

        var parts = geolocationStr.Split("|");
        var vo = new IpGeolocationVo
        {
            IpAddress = ipAddress,
            IsInternalIp = false
        };

        var index = 0;
        foreach (var part in parts)
        {
            switch (index)
            {
                case 0:
                    vo.Country = Normalize(part);
                    break;
                case 2:
                    vo.Province = Normalize(part);
                    break;
                case 3:
                    vo.City = Normalize(part);
                    break;
                case 4:
                    vo.Isp = Normalize(part);
                    break;
            }
            index++;
        }

        return vo;
    }

    /// <summary>
    ///     xdb 对缺失字段返回 "0"，归一化为 null 避免污染展示与统计
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string Normalize(string value)
    {
        return string.IsNullOrEmpty(value) || value == "0" ? null : value;
    }

    /// <summary>
    ///     批量查询IP地址的地理位置信息
    /// </summary>
    /// <param name="ipAddresses">ipAddresses IP地址列表</param>
    /// <returns>地理位置信息映射表</returns>
    public Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses)
    {
        var dict = new Dictionary<string, IpGeolocationVo>();
        foreach (var item in ipAddresses)
        {
            //  跳过 null/空白项，避免 Dictionary 键异常
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            dict[item] = GetLocationByIp(item);
        }
        return dict;
    }
}