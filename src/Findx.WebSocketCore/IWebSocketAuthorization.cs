using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore;

/// <summary>
/// WebSocket鉴权
/// </summary>
public interface IWebSocketAuthorization
{
   /// <summary>
   /// 鉴权
   /// </summary>
   /// <param name="request"></param>
   /// <returns></returns>
   Task<bool> AuthorizeAsync(HttpRequest request);
}