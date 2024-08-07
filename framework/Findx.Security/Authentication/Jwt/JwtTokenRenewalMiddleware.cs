using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     Jwt Token 自动续期中间件
/// </summary>
public class JwtTokenRenewalMiddleware
{
    private readonly ILogger<JwtTokenRenewalMiddleware> _logger;
    private readonly RequestDelegate _next;
    private static readonly List<string> ClaimIgnoreKeys = ["nbf", "exp", "iat", "iss", "aud"];
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public JwtTokenRenewalMiddleware(RequestDelegate next, ILogger<JwtTokenRenewalMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    ///     执行中间件拦截逻辑
    /// </summary>
    /// <param name="context">Http上下文</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // 自动续期控制
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var options = context.RequestServices.GetRequiredService<IOptions<JwtOptions>>();
            var exp = context.User.Identity.GetClaimValueFirstOrDefault("exp");
            // 判断是否符合续期条件
            if (options.Value.RenewalMinutes > 0 && !exp.IsNullOrWhiteSpace())
            {
                var spanTime = DateTimeOffset.FromUnixTimeSeconds(exp.To<long>()).LocalDateTime -
                               DateTimeOffset.Now.LocalDateTime;
                if (spanTime.TotalMinutes <= options.Value.RenewalMinutes)
                {
                    var renewalDict = context.User.Claims.Where(claim => !ClaimIgnoreKeys.Contains(claim.Type)).ToDictionary(claim => claim.Type, claim => claim.Value);
                    var jwtBuilder = context.RequestServices.GetRequiredService<IJwtTokenBuilder>();
                    var token = await jwtBuilder.CreateAsync(renewalDict, options.Value);
                    context.Response.Headers.Append("Authorization", $"Bearer {token.AccessToken}");
                }
            }
        }
        await _next(context);
    }
}