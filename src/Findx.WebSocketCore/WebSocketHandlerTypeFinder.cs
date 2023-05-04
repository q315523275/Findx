using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Reflection;

namespace Findx.WebSocketCore;

/// <summary>
///     查找器
/// </summary>
public class WebSocketHandlerTypeFinder : BaseTypeFinderBase<WebSocketHandlerBase>, IWebSocketHandlerTypeFinder
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public WebSocketHandlerTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
    {
    }

    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        // 排除被继承的Handler实类
        var types = base.FindAllItems();
        var baseHandlerTypes = types.Select(m => m.BaseType).Where(m => m != null && m.IsClass && !m.IsAbstract);
        return types.Except(baseHandlerTypes);
    }
}