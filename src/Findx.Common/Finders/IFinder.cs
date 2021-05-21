using System;

namespace Findx.Finders
{
    public interface IFinder<out TItem>
    {
        // <summary>
        /// 查找指定条件的项
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <param name="fromCache">是否来自缓存</param>
        /// <returns></returns>
        TItem[] Find(Func<TItem, bool> predicate, bool fromCache = false);

        /// <summary>
        /// 查找所有项
        /// </summary>
        /// <param name="fromCache">是否来自缓存</param>
        /// <returns></returns>
        TItem[] FindAll(bool fromCache = false);
    }
}
