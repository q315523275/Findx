using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Security.Authentication.Jwt;
using Findx.WebSocketCore.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Findx.WebSocketCore.Implementation;

/// <summary>
///     WebSocket鉴权
/// </summary>
public class DefaultWebSocketAuthorization : IWebSocketAuthorization
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    public DefaultWebSocketAuthorization(IOptions<JwtOptions> options)
    {
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = options.Value.Issuer ?? "findx",
            ValidAudience = options.Value.Audience ?? "findx",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret)),
            LifetimeValidator = (_, exp, _, _) => exp > DateTimeOffset.UtcNow
        };
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    ///     鉴权
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task<bool> AuthorizeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            // 手动验证Token
            var principal = _jwtSecurityTokenHandler.ValidateToken(token.ToString().RemovePreFix("Bearer "), _tokenValidationParameters, out _);
            
            // 将用户信息附加到上下文
            context.User = principal;
            
            // 认证结果
            var result = context.User.Identity?.IsAuthenticated ?? false;
            
            return Task.FromResult(result);
        }
        
        return Task.FromResult(true);
    }
}