using System;
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

    public string GetLocationTextByIp(string ipAddress)
    {
        if (NetUtility.IsInternalIp(ipAddress))
        {
            return "局域网";
        }
        
        return _searcher.Search(ipAddress);
    }

    public Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses)
    {
        var dict = new Dictionary<string, string>();
        foreach (var item in ipAddresses)
        {
            dict[item] = GetLocationTextByIp(item);
        }
        return dict;
    }

    public IpGeolocationVo GetLocationByIp(string ipAddress)
    {
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
                    vo.Country = part;
                    break;
                case 2:
                    vo.Province = part;
                    break;
                case 3:
                    vo.City = part;
                    break;
                case 4:
                    vo.Isp = part;
                    break;
            }
            index++;
        }
        
        return vo;
        
    }

    public Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses)
    {
        var dict = new Dictionary<string, IpGeolocationVo>();
        foreach (var item in ipAddresses)
        {
            dict[item] = GetLocationByIp(item);
        }
        return dict;
    }
}