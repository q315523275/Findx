using Findx.Extensions;
using Findx.Finders;
using Findx.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Findx.DependencyInjection
{
    public class DependencyTypeFinder : FinderBase<Type>, IDependencyTypeFinder
    {
        private readonly IAppDomainAssemblyFinder _appDomainAssemblyFinder;
        public DependencyTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder)
        {
            _appDomainAssemblyFinder = appDomainAssemblyFinder;
        }
        /// <summary>
        /// 重写以实现所有项的查找
        /// </summary>
        /// <returns></returns>
        protected override Type[] FindAllItems()
        {
            Type[] baseTypes = new[] { typeof(ISingletonDependency), typeof(IScopeDependency), typeof(ITransientDependency) };
            Assembly[] assemblies = _appDomainAssemblyFinder.FindAll(true);
            return assemblies.SelectMany(assemblie => assemblie.GetTypes())
                             .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && !type.HasAttribute<IgnoreDependencyAttribute>()
                             && (baseTypes.Any(b => b.IsAssignableFrom(type)) || type.HasAttribute<DependencyAttribute>())).Distinct().ToArray();
        }
    }
}
