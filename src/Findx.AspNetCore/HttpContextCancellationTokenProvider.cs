using System.Threading;
using Findx.Threading;
using Microsoft.AspNetCore.Http;

namespace Findx.AspNetCore;

/// <summary>
///     取消令牌提供器
/// </summary>
public class HttpContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    ///     Token令牌
    /// </summary>
    public CancellationToken Token => _httpContextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
}