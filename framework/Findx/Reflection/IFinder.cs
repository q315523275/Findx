using Findx.DependencyInjection;

namespace Findx.Reflection;

/// <summary>
///     反射查找器
/// </summary>
/// <typeparam name="TItem"></typeparam>
[IgnoreDependency]
public interface IFinder<out TItem>
{
    /// <summary>
    ///     查找指定条件的项
    /// </summary>
    /// <param name="predicate">筛选条件</param>
    /// <param name="fromCache">是否来自缓存</param>
    /// <returns></returns>
    IEnumerable<TItem> Find(Func<TItem, bool> predicate, bool fromCache = false);

    /// <summary>
    ///     查找所有项
    /// </summary>
    /// <param name="fromCache">是否来自缓存</param>
    /// <returns></returns>
    IEnumerable<TItem> FindAll(bool fromCache = false);
}