using System;
using System.Collections.Generic;
using Findx.Finders;
using Findx.Reflection;

namespace Findx.WebApiClient;

public class WebApiFinder : FinderBase<Type>, IWebApiFinder
{
    protected override IEnumerable<Type> FindAllItems()
    {
        return AssemblyManager.FindTypesByAttribute<WebApiClientAttribute>();
    }
}