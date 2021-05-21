using Findx.Reflection;
using System;
using System.Linq;

namespace Findx.Modularity
{
    public class FindxModuleTypeFinder : BaseTypeFinderBase<FindxModule>, IFindxModuleTypeFinder
    {
        public FindxModuleTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
        {
        }
        /// <summary>
        /// 重写以实现所有项的查找
        /// </summary>
        /// <returns></returns>
        protected override Type[] FindAllItems()
        {
            // 排除被继承的Module实类
            Type[] types = base.FindAllItems();
            Type[] baseModuleTypes = types.Select(m => m.BaseType).Where(m => m != null && m.IsClass && !m.IsAbstract).ToArray();
            return types.Except(baseModuleTypes).ToArray();
        }
    }
}
