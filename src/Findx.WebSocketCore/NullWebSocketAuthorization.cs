using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore;

/// <summary>
///     WebSocket鉴权
/// </summary>
public class NullWebSocketAuthorization : IWebSocketAuthorization
{
    /// <summary>
    ///     鉴权
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<bool> AuthorizeAsync(HttpRequest request)
    {
        return Task.FromResult(true);
    }
}