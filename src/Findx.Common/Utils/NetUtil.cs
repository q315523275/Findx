using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Findx.Utils
{
    /// <summary>
    /// 网络辅助工具类
    /// </summary>
    public static class NetUtil
    {
        /// <summary>
        /// 是否能Ping通指定主机
        /// </summary>
        public static bool Ping(string ip)
        {
            try
            {
                Ping ping = new Ping();
                PingOptions options = new PingOptions { DontFragment = true };
                string data = "Test Data";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000;
                PingReply reply = ping.Send(ip, timeout, buffer, options);
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return false;
            }
        }

        /// <summary>
        /// 是否内网IP
        /// </summary>
        /// <param name="ipv4Address"></param>
        /// <returns></returns>
        public static bool IsInternalIP(string ipv4Address)
        {
            if (IPAddress.TryParse(ipv4Address, out var ip))
            {
                byte[] ipBytes = ip.GetAddressBytes();
                if (ipBytes[0] == 10) return true;
                if (ipBytes[0] == 127 && ipBytes[1] == 0) return true;
                if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true;
                if (ipBytes[0] == 192 && ipBytes[1] == 168) return true;
            }
            return false;
        }

        /// <summary>
        /// 是否内网IP
        /// </summary>
        /// <param name="ipv4Address"></param>
        /// <returns></returns>
        public static bool IsInternalIP(IPAddress ipv4Address)
        {
            byte[] ipBytes = ipv4Address.GetAddressBytes();
            if (ipBytes[0] == 10) return true;
            if (ipBytes[0] == 127 && ipBytes[1] == 0) return true;
            if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true;
            if (ipBytes[0] == 192 && ipBytes[1] == 168) return true;
            return false;
        }
    }
}
