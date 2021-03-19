using Microsoft.Extensions.Options;

namespace Findx.Security.Authentication.Jwt
{
    public class JwtOptions : IOptions<JwtOptions>
    {
        /// <summary>
        /// 获取或设置 密钥
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 获取或设置 发行方
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 获取或设置 订阅方
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 获取或设置 AccessToken有效期分钟数
        /// </summary>
        public double AccessExpireMins { get; set; }

        /// <summary>
        /// 获取或设置 RefreshToken有效期分钟数
        /// </summary>
        public double RefreshExpireMins { get; set; }

        /// <summary>
        /// 获取或设置 RefreshToken是否绝对过期
        /// </summary>
        public bool IsRefreshAbsoluteExpired { get; set; } = true;

        /// <summary>
        /// 获取或设置 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        public JwtOptions Value => this;
    }
}
