using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
namespace Findx.Security.Authorization
{
    /// <summary>
    /// 授权处理器
    /// </summary>
    public sealed class FunctionRequirementHandler : AuthorizationHandler<FunctionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFunctionAuthorization _functionAuthorization;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="functionAuthorization"></param>
        public FunctionRequirementHandler(IHttpContextAccessor httpContextAccessor, IFunctionAuthorization functionAuthorization)
        {
            _httpContextAccessor = httpContextAccessor;
            _functionAuthorization = functionAuthorization;
        }

        /// <summary>
        /// 根据特定要求允许授权，则作出决定
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FunctionRequirement requirement)
        {
            RouteEndpoint endpoint = null;
            HttpContext httpContext = null;
            switch (context.Resource)
            {
                case HttpContext resource1:
                    httpContext = resource1;
                    endpoint = httpContext.GetEndpoint() as RouteEndpoint;
                    break;
                case RouteEndpoint resource2:
                    httpContext = _httpContextAccessor.HttpContext;
                    endpoint = resource2;
                    break;
            }

            if (endpoint == null || httpContext == null)
            {
                return Task.CompletedTask;
            }
            
            var function = endpoint.GetExecuteFunction(httpContext);
            if (AuthorizeCore(context, function) == AuthorizationStatus.Ok)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// 重写以实现功能权限的核心验证逻辑
        /// </summary>
        /// <param name="context">权限过滤器上下文</param>
        /// <param name="function">要验证的功能</param>
        /// <returns>权限验证结果</returns>
        private AuthorizationStatus AuthorizeCore(AuthorizationHandlerContext context, IFunction function)
        {
            return _functionAuthorization.Authorize(function, context.User);
        }
    }
}
