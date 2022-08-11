using Findx.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Findx.AspNetCore
{
    /// <summary>
    /// Http请求服务范围工厂
    /// </summary>
    public class HttpContextServiceScopeFactory : IHybridServiceScopeFactory
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="httpContextAccessor"></param>
        public HttpContextServiceScopeFactory(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            ServiceScopeFactory = serviceScopeFactory;
            HttpContextAccessor = httpContextAccessor;
        }
        
        /// <summary>
        /// 服务范围工厂
        /// </summary>
        private IServiceScopeFactory ServiceScopeFactory { get; }
        
        /// <summary>
        /// Http上下文访问器
        /// </summary>
        private IHttpContextAccessor HttpContextAccessor { get; }
        
        /// <summary>
        /// 创建作用域
        /// </summary>
        /// <returns></returns>
        public virtual IServiceScope CreateScope()
        {
            var httpContext = HttpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                return ServiceScopeFactory.CreateScope();
            }
            return new NonDisposedHttpContextServiceScope(httpContext.RequestServices);
        }
        
        /// <summary>
        /// 
        /// </summary>
        private class NonDisposedHttpContextServiceScope : IServiceScope
        {
            /// <summary>
            /// 服务提供商
            /// </summary>
            public IServiceProvider ServiceProvider { get; }
            
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="serviceProvider"></param>
            public NonDisposedHttpContextServiceScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose() { }
        }
    }
}
