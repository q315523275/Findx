using Findx.Finders;
using Findx.Reflection;

namespace Findx.Jobs.Client;

/// <summary>
///     循环调度任务(IScheduledTask)查询器
/// </summary>
public class JobFinder : FinderBase<Type>, IJobFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        var baseTypes = typeof(IJob);
        return AssemblyManager.FindTypesByBase(baseTypes);
    }
}