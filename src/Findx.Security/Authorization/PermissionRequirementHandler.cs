using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
namespace Findx.Security.Authorization
{
    /// <summary>
    /// 授权处理器
    /// </summary>
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionChecker _permissionChecker;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="permissionChecker"></param>
        /// <param name="httpContextAccessor"></param>
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
                PermissionAccess permissionAccess = endpoint.GetExecutePermissionAccess(httpContext);
                if (await _permissionChecker.IsGrantedAsync(httpContext.User, permissionAccess))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (context.Resource is DefaultHttpContext defaultHttpContext)
            {
                PermissionAccess permissionAccess = defaultHttpContext.GetExecutePermissionAccess();
                if (await _permissionChecker.IsGrantedAsync(httpContext.User, permissionAccess))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
