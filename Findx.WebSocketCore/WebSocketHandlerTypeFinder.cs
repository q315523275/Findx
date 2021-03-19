using Findx.Reflection;
using System;
using System.Linq;

namespace Findx.WebSocketCore
{
    public class WebSocketHandlerTypeFinder : BaseTypeFinderBase<WebSocketHandler>, IWebSocketHandlerTypeFinder
    {
        public WebSocketHandlerTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
        {
        }
        /// <summary>
        /// 重写以实现所有项的查找
        /// </summary>
        /// <returns></returns>
        protected override Type[] FindAllItems()
        {
            // 排除被继承的Handler实类
            Type[] types = base.FindAllItems();
            Type[] baseHandlerTypes = types.Select(m => m.BaseType).Where(m => m != null && m.IsClass && !m.IsAbstract).ToArray();
            return types.Except(baseHandlerTypes).ToArray();
        }
    }
}
