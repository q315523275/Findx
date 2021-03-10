using Findx.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore
{
    public class HttpContextServiceScopeResolver : IScopedServiceResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextServiceScopeResolver(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public bool ResolveEnabled => _httpContextAccessor.HttpContext != null;

        public T GetService<T>()
        {
            return _httpContextAccessor.HttpContext.RequestServices.GetService<T>();
        }
    }
}
