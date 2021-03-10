using Microsoft.Extensions.DependencyInjection;

namespace Findx.DependencyInjection
{
    public class DefaultServiceScopeFactory : IHybridServiceScopeFactory
    {
        public DefaultServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        /// <summary>
        /// 创建新的容器作用域
        /// </summary>
        /// <returns></returns>
        public IServiceScope CreateScope()
        {
            return ServiceScopeFactory.CreateScope();
        }
    }
}
