using System;

namespace Findx.Security.Authentication.Jwt
{
    /// <summary>
    /// JwtToken
    /// </summary>
    public class JwtToken
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 访问令牌有效期
        /// </summary>
        public DateTime? AccessTokenExpires { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// 刷新令牌有效期
        /// </summary>
        public DateTime? RefreshTokenExpires { get; set; }
    }
}
