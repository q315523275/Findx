using System.Threading.Tasks;
using Findx.WebSocketCore.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore.Implementation;

/// <summary>
///     WebSocket鉴权
/// </summary>
public class NullWebSocketAuthorization : IWebSocketAuthorization
{
    /// <summary>
    ///     鉴权
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task<bool> AuthorizeAsync(HttpContext context)
    {
        return Task.FromResult(true);
    }
}