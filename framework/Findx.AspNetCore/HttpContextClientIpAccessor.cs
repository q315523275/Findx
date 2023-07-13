using Findx.AspNetCore.Extensions;
using Findx.Threading;
using Microsoft.AspNetCore.Http;

namespace Findx.AspNetCore;

/// <summary>
/// Http请求客户ip访问器材
/// </summary>
public class HttpContextClientIpAccessor: IThreadCurrentClientIpAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public HttpContextClientIpAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取客户端Ip
    /// </summary>
    /// <returns></returns>
    public string GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.GetClientIp();
    }
}