namespace Findx.Net.IP;

public class NullIpGeolocation: IIpGeolocation
{
    public string GetLocationTextByIp(string ipAddress)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses)
    {
        throw new NotImplementedException();
    }

    public IpGeolocationVo GetLocationByIp(string ipAddress)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses)
    {
        throw new NotImplementedException();
    }
}