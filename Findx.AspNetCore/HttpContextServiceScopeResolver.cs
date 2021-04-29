using Findx.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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

        public object GetService(Type serviceType)
        {
            return _httpContextAccessor.HttpContext.RequestServices.GetService(serviceType);
        }

        public IEnumerable<T> GetServices<T>()
        {
            return _httpContextAccessor.HttpContext.RequestServices.GetServices<T>();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _httpContextAccessor.HttpContext.RequestServices.GetServices(serviceType);
        }
    }
}
