using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// AspNetCore扩展 - Request
    /// </summary>
    public static partial class Extensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            Check.NotNull(request, nameof(request));

            return "XMLHttpRequest".Equals(request.Headers["X-Requested-With"], StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public static string GetClientIp(this HttpContext context)
        {
            string ip = context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = context.Request.Headers["REMOTE_ADDR"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ip))
            {
                IPAddress remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress.IsIPv4MappedToIPv6)
                {
                    ip = remoteIpAddress.MapToIPv4().ToString();
                }

                if (string.IsNullOrEmpty(ip) && IPAddress.IsLoopback(remoteIpAddress))
                {
                    return "127.0.0.1";
                }
            }

            Check.NotNullOrWhiteSpace(ip, nameof(ip));

            return ip;
        }
    }
}
