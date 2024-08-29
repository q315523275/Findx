using Findx.Common;

namespace Findx.Data.Compare;

/// <summary>
///     键列表比较器
/// </summary>
/// <typeparam name="TKey">标识类型</typeparam>
public class KeyListComparator<TKey>
{
    /// <summary>
    ///     比较
    /// </summary>
    /// <param name="newList">新实体集合</param>
    /// <param name="originalList">旧实体集合</param>
    public KeyListCompareResult<TKey> Compare(IEnumerable<TKey> newList, IEnumerable<TKey> originalList)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        newList.ThrowIfNull(nameof(newList));
        // ReSharper disable once PossibleMultipleEnumeration
        originalList.ThrowIfNull(nameof(originalList));
        // ReSharper disable once PossibleMultipleEnumeration
        var newEntities = newList.ToList();
        // ReSharper disable once PossibleMultipleEnumeration
        var originalEntities = originalList.ToList();
        var createList = GetCreateList(newEntities, originalEntities);
        var updateList = GetUpdateList(newEntities, originalEntities);
        var deleteList = GetDeleteList(newEntities, originalEntities);
        return new KeyListCompareResult<TKey>(createList, updateList, deleteList);
    }

    /// <summary>
    ///     获取创建列表
    /// </summary>
    private static List<TKey> GetCreateList(List<TKey> newList, List<TKey> originalList)
    {
        var result = newList.Except(originalList);
        return result.ToList();
    }

    /// <summary>
    ///     获取更新列表
    /// </summary>
    private static List<TKey> GetUpdateList(List<TKey> newList, List<TKey> originalList)
    {
        return newList.FindAll(id => originalList.Exists(t => t.Equals(id)));
    }

    /// <summary>
    ///     获取删除列表
    /// </summary>
    private static List<TKey> GetDeleteList(List<TKey> newList, List<TKey> originalList)
    {
        var result = originalList.Except(newList);
        return result.ToList();
    }
}