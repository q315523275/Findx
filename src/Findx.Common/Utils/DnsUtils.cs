using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Findx.Utils
{
    public static class DnsUtils
    {
        public static string ResolveHostAddress(string hostName)
        {
            try
            {
                return Dns.GetHostAddresses(hostName).FirstOrDefault(ip => ip.AddressFamily.Equals(AddressFamily.InterNetwork))?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string ResolveHostName()
        {
            string result = null;
            try
            {
                result = Dns.GetHostName();
                if (!string.IsNullOrEmpty(result))
                {
                    var response = Dns.GetHostEntry(result);
                    if (response != null)
                    {
                        return response.HostName;
                    }
                }
            }
            catch { }

            return result;
        }
    }
}
