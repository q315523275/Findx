using Findx.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx.Modularity
{
    /// <summary>
    /// Findx框架模块查找器
    /// </summary>
    public class FindxModuleTypeFinder : BaseTypeFinderBase<FindxModule>, IFindxModuleTypeFinder
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="appDomainAssemblyFinder"></param>
        public FindxModuleTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
        {
        }
        /// <summary>
        /// 重写以实现所有项的查找
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Type> FindAllItems()
        {
            // 排除被继承的Module实类
            var types = base.FindAllItems();
            var baseModuleTypes = types.Select(m => m.BaseType).Where(m => m != null && m.IsClass && !m.IsAbstract);
            return types.Except(baseModuleTypes);
        }
    }
}
