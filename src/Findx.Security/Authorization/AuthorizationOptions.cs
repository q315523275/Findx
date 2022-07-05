using Microsoft.Extensions.Options;

namespace Findx.Security.Authorization
{
    public class AuthorizationOptions : IOptions<AuthorizationOptions>
    {
        public AuthorizationOptions Value => this;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { set; get; }

        /// <summary>
        /// 校验客户端IP变更
        /// </summary>
        public bool VerifyClientIpChanged { set; get; }
    }
}
