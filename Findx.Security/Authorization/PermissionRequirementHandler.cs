using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Findx.Authorization
{
    /// <summary>
    /// 授权处理器
    /// </summary>
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionChecker _permissionChecker;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionRequirementHandler(IPermissionChecker permissionChecker, IHttpContextAccessor httpContextAccessor)
        {
            _permissionChecker = permissionChecker;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 根据特定要求允许授权，则作出决定
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (context.Resource is RouteEndpoint endpoint)
            {
                ClaimsPrincipal principal = httpContext.User;
                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    context.Fail();
                    return;
                }

                // 单设备登录
                if (requirement.SingleDeviceEnabled)
                {

                }
                // 授权验证
                if (await _permissionChecker.IsGrantedAsync(context.User, httpContext))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
