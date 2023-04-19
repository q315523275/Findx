using Findx.DependencyInjection;
using System.Security.Claims;
using System.Security.Principal;

namespace Findx.Security
{
    /// <summary>
    /// 当前用户
    /// </summary>
    public class CurrentUser : ICurrentUser, ITransientDependency
    {
        private readonly IPrincipal _principal;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="principal"></param>
        public CurrentUser(IPrincipal principal)
        {
            _principal = principal;
        }

        /// <summary>
        /// 是否认证通过
        /// </summary>
        public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId => _principal?.Identity?.GetUserId();

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName => this.FindClaimValue(ClaimTypes.UserName);

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name => this.FindClaimValue(ClaimTypes.Name);
        
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname => this.FindClaimValue(ClaimTypes.Nickname);

        /// <summary>
        /// 手机号
        /// </summary>
        public string PhoneNumber => this.FindClaimValue(ClaimTypes.PhoneNumber);

        /// <summary>
        /// 手机号是否已验证
        /// </summary>
        public bool PhoneNumberVerified => string.Equals(this.FindClaimValue(ClaimTypes.PhoneNumberVerified), "true", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email => this.FindClaimValue(ClaimTypes.Email);

        /// <summary>
        /// 邮箱是否已验证
        /// </summary>
        public bool EmailVerified => string.Equals(this.FindClaimValue(ClaimTypes.EmailVerified), "true", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// 租户编号
        /// </summary>
        public string TenantId => _principal?.Identity?.GetClaimValueFirstOrDefault(ClaimTypes.TenantId);

        /// <summary>
        /// 角色集合
        /// </summary>
        public IEnumerable<string> Roles => FindClaims(ClaimTypes.Role).Select(c => c.Value).Distinct();



        /// <summary>
        /// 查找声明
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public Claim FindClaim(string claimType)
        {
            return _principal?.Identity?.GetClaimFirstOrDefault(claimType);
        }

        /// <summary>
        /// 查找声明集合
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public IEnumerable<Claim> FindClaims(string claimType)
        {
            return _principal?.Identity?.GetClaims(claimType) ?? Array.Empty<Claim>();
        }

        /// <summary>
        /// 获取所有声明
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Claim> GetAllClaims()
        {
            if (_principal.Identity is not ClaimsIdentity claimsIdentity)
            {
                return Array.Empty<Claim>();
            }
            return claimsIdentity.Claims;
        }

        /// <summary>
        /// 判断是否包含某角色
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsInRole(string roleName)
        {
            return _principal?.Identity?.GetClaimValues(System.Security.Claims.ClaimTypes.Role)?.Any(c => c == roleName) ?? false;
        }
    }
}
