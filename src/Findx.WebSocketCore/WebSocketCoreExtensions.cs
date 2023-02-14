using System.Net;
using Findx.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class WebSocketCoreExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app, PathString path, WebSocketHandlerBase handler)
        {
            return app.Map(path, (x) => x.UseMiddleware<WebSocketManagerMiddleware>(handler));
        }
        
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public static string GetClientIp(this HttpContext context)
        {
            var ip = context.Request.Headers.GetOrDefault("X-Forwarded-For").SafeString().Split(',')[0];
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = context.Request.Headers.GetOrDefault("REMOTE_ADDR");
            }
            if (string.IsNullOrEmpty(ip))
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress is { IsIPv4MappedToIPv6: true })
                {
                    ip = remoteIpAddress.MapToIPv4().ToString();
                }

                if (string.IsNullOrEmpty(ip) && remoteIpAddress != null && IPAddress.IsLoopback(remoteIpAddress))
                {
                    return "127.0.0.1";
                }
            }

            Check.NotNullOrWhiteSpace(ip, nameof(ip));

            return ip;
        }
    }
}
