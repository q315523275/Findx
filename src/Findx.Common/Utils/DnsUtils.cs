﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Findx.Utils
{
    /// <summary>
    /// DNS工具类
    /// </summary>
    public static class DnsUtils
    {
        /// <summary>
        /// 解析主机地址
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 解析主机名
        /// </summary>
        /// <returns></returns>
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
