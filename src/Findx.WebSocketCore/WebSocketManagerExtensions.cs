using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class WebSocketManagerExtensions
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
    }
}
