using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using Findx.Extensions;
namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// AspNetCore扩展 - Request
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 确定指定的 HTTP 请求是否为 AJAX 请求。
        /// </summary>
        ///
        /// <returns>
        /// 如果指定的 HTTP 请求是 AJAX 请求，则为 true；否则为 false。
        /// </returns>
        /// <param name="request">HTTP 请求。</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="request"/> 参数为 null（在 Visual Basic 中为 Nothing）。</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            Check.NotNull(request, nameof(request));

            return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal)
                || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }
        /// <summary>
        /// 确定指定的 HTTP 请求的 ContextType 是否为 Json 方式
        /// </summary>
        public static bool IsJsonContextType(this HttpRequest request)
        {
            Check.NotNull(request, nameof(request));
            bool flag = request.Headers?["Content-Type"].ToString().IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1
                || request.Headers?["Content-Type"].ToString().IndexOf("text/json", StringComparison.OrdinalIgnoreCase) > -1;
            if (flag)
            {
                return true;
            }
            flag = request.Headers?["Accept"].ToString().IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1
                || request.Headers?["Accept"].ToString().IndexOf("text/json", StringComparison.OrdinalIgnoreCase) > -1;
            return flag;
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

        /// <summary>
        /// 获取请求浏览器
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetBrowser(this HttpRequest request)
        {
            var userAgent = request.Headers?["User-Agent"].SafeString();
            return new Findx.Utils.UserAgent(userAgent).GetBrowser();
        }
        /// <summary>
        /// 获取请求系统
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetSystem(this HttpRequest request)
        {
            var userAgent = request.Headers?["User-Agent"].SafeString();
            return new Findx.Utils.UserAgent(userAgent).GetSystem();
        }
    }
}
