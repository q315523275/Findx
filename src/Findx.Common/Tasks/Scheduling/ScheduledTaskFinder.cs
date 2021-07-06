using Findx.Finders;
using Findx.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 循环调度任务(IScheduledTask)查询器
    /// </summary>
    public class ScheduledTaskFinder : FinderBase<Type>, IScheduledTaskFinder
    {
        private readonly IAppDomainAssemblyFinder _appDomainAssemblyFinder;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="appDomainAssemblyFinder"></param>
        public ScheduledTaskFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder)
        {
            _appDomainAssemblyFinder = appDomainAssemblyFinder;
        }

        /// <summary>
        /// 重写以实现所有项的查找
        /// </summary>
        /// <returns></returns>
        protected override Type[] FindAllItems()
        {
            Type baseTypes = typeof(IScheduledTask);
            Assembly[] assemblies = _appDomainAssemblyFinder.FindAll(true);
            return assemblies.SelectMany(assemblie => assemblie.GetTypes())
                             .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && baseTypes.IsAssignableFrom(type)).Distinct().ToArray();
        }
    }
}
