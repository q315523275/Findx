using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Reflection;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore.Implementation;

/// <summary>
///     查找器
/// </summary>
public class WebSocketHandlerTypeFinder : FinderBase<Type>, IWebSocketHandlerTypeFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        // 排除被继承的Handler实类
        var types = AssemblyManager.FindTypesByBase<WebSocketHandlerBase>();
        var baseHandlerTypes = types.Select(m => m.BaseType).Where(m => m is { IsClass: true, IsAbstract: false });
        return types.Except(baseHandlerTypes);
    }
}