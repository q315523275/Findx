using Findx.Reflection;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 循环调度任务注解(ScheduledAttribute)查询器
    /// </summary>
    public class ScheduledTaskFinder : AttributeTypeFinderBase<ScheduledAttribute>, IScheduledTaskFinder
    {
        public ScheduledTaskFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder) { }
    }
}
