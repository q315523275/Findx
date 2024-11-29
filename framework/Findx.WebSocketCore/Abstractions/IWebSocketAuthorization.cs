using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore.Abstractions;

/// <summary>
///     WebSocket鉴权
/// </summary>
public interface IWebSocketAuthorization
{
   /// <summary>
   ///     鉴权
   /// </summary>
   /// <param name="context"></param>
   /// <returns></returns>
   Task<bool> AuthorizeAsync(HttpContext context);
}
