using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     默认token生成器
/// </summary>
public class DefaultJwtTokenBuilder : IJwtTokenBuilder
{
    /// <summary>
    ///     Jwt安全令牌处理器
    /// </summary>
    private readonly JwtSecurityTokenHandler _tokenHandler;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DefaultJwtTokenBuilder()
    {
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    ///     创建
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="jwtOption"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task<JwtToken> CreateAsync(IDictionary<string, string> payload, JwtOptions jwtOption)
    {
        Check.NotNull(jwtOption, nameof(jwtOption));
        Check.NotNull(jwtOption.Secret, nameof(jwtOption.Secret));

        // 客户端信息
        // var clientId = payload.ContainsKey(ClaimTypes.ClientId) ? payload[ClaimTypes.ClientId] : Guid.NewGuid().ToString();
        // var clientType = payload.ContainsKey(ClaimTypes.ClientType) ? payload[ClaimTypes.ClientType] : "admin";

        if (!payload.ContainsKey(ClaimTypes.UserId))
            throw new ArgumentException("不存在用户标识");

        var claims = JwtTokenHelper.ToClaims(payload);

        // 不支持刷新
        // 生成刷新令牌
        // var (refreshToken, refreshExpires) = Helper.CreateToken(_tokenHandler, claims, jwtOption, JsonWebTokenType.RefreshToken);

        // 生成访问令牌
        var (token, accessExpires) = JwtTokenHelper.CreateToken(_tokenHandler, claims, jwtOption, JsonWebTokenType.AccessToken);

        var accessToken = new JwtToken
        {
            AccessToken = token,
            AccessTokenUtcExpires = accessExpires.ToJsGetTime().To<long>()
            // RefreshToken = refreshToken,
            // RefreshUtcExpires = refreshExpires.ToJsGetTime().To<long>()
        };
        return Task.FromResult(accessToken);
    }
}