using System;
using Findx.Extensions;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     JwtToken
/// </summary>
public class JwtToken
{
    /// <summary>
    ///     访问令牌
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    ///     访问令牌有效期
    /// </summary>
    public long AccessTokenUtcExpires { get; set; }

    /// <summary>
    ///     刷新令牌
    /// </summary>
    public string RefreshToken { get; set; }
    
    /// <summary>
    ///     刷新令牌有效期
    /// </summary>
    public long RefreshUtcExpires { get; set; }

    /// <summary>
    ///     是否已过期
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow.ToJsGetTime().To<long>() > AccessTokenUtcExpires;
    }
}