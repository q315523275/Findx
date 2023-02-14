using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Findx.Security.Authentication.Jwt
{
    /// <summary>
    /// JwtBearer帮助类
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// 转换为声明列表
        /// </summary>
        /// <param name="dictionary">字典</param>
        public static IEnumerable<Claim> ToClaims(IDictionary<string, string> dictionary) => dictionary.Keys.Select(key => new Claim(key, dictionary[key]?.ToString() ?? string.Empty));

        /// <summary>
        /// 创建令牌
        /// </summary>
        /// <param name="tokenHandler">Jwt安全令牌处理器</param>
        /// <param name="claims">声明列表</param>
        /// <param name="options">Jwt选项配置</param>
        /// <param name="tokenType">Jwt令牌类型</param>
        public static (string token, DateTime expires) CreateToken(JwtSecurityTokenHandler tokenHandler, IEnumerable<Claim> claims, JwtOptions options, JsonWebTokenType tokenType)
        {
            var secret = options.Secret;
            Check.NotNull(secret, nameof(secret));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var minutes = tokenType == JsonWebTokenType.AccessToken
                ? options.AccessExpireMinutes > 0 ? options.AccessExpireMinutes : 5 // 默认5分钟
                : options.RefreshExpireMinutes > 0
                    ? options.RefreshExpireMinutes
                    : 10080; // 默认7天
            var expires = now.AddMinutes(minutes);
            var jwt = new JwtSecurityToken(options.Issuer, options.Audience, claims, now.AddMinutes(-1), expires, credentials);
            var accessToken = tokenHandler.WriteToken(jwt);
            return (accessToken, expires);
        }
    }
    /// <summary>
    /// Jwt令牌类型
    /// </summary>
    internal enum JsonWebTokenType
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        AccessToken,

        /// <summary>
        /// 刷新令牌
        /// </summary>
        RefreshToken
    }
}
