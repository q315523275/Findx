using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Extensions;
using Findx.Finders;
using Findx.Reflection;

namespace Findx.WebApiClient;

public class WebApiFinder : FinderBase<Type>, ITypeFinder, IWebApiFinder
{
    private readonly IAppDomainAssemblyFinder _appDomainAssemblyFinder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public WebApiFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder)
    {
        _appDomainAssemblyFinder = appDomainAssemblyFinder;
    }

    protected override IEnumerable<Type> FindAllItems()
    {
        var assemblies = _appDomainAssemblyFinder.FindAll(true);
        return assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsInterface && type.HasAttribute<WebApiClientAttribute>()).Distinct();
    }
}