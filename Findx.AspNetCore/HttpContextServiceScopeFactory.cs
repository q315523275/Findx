using Findx.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Findx.AspNetCore
{
    public class HttpContextServiceScopeFactory : IHybridServiceScopeFactory
    {
        public HttpContextServiceScopeFactory(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            ServiceScopeFactory = serviceScopeFactory;
            HttpContextAccessor = httpContextAccessor;
        }
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected IHttpContextAccessor HttpContextAccessor { get; }
        public virtual IServiceScope CreateScope()
        {
            HttpContext httpContext = HttpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                return ServiceScopeFactory.CreateScope();
            }
            return new NonDisposedHttpContextServiceScope(httpContext.RequestServices);
        }
        protected class NonDisposedHttpContextServiceScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }
            public NonDisposedHttpContextServiceScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }
            public void Dispose() { }
        }
    }
}
