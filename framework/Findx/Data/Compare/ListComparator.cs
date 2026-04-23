using Findx.Common;

namespace Findx.Data.Compare;

/// <summary>
///     实体列表比较器
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">标识类型</typeparam>
public class ListComparator<TEntity, TKey> where TEntity : IEntity<TKey>
{
    /// <summary>
    ///     比较两个实体集合，返回需要创建、更新、删除的实体列表
    /// </summary>
    /// <param name="newList">新实体集合</param>
    /// <param name="originalList">旧实体集合</param>
    /// <returns>比较结果，包含三个列表</returns>
    public ListCompareResult<TEntity, TKey> Compare(List<TEntity> newList, List<TEntity> originalList)
    {
        //  构建旧实体的字典，以 Id 为键
        var originalDict = originalList.ToDictionary(x => x.Id);

        //  预分配容量
        var newIdSet = new HashSet<TKey>(newList.Count);
        var createList = new List<TEntity>(newList.Count);
        var updateList = new List<TEntity>(newList.Count);

        //  一次遍历 newList，完成新 Id 集合构建及分类
        foreach (var entity in newList)
        {
            newIdSet.Add(entity.Id);
            //  使用 TryGetValue 仅一次字典查找
            if (originalDict.TryGetValue(entity.Id, out _))
            {
                updateList.Add(entity);
            }
            else
            {
                createList.Add(entity);
            }
        }

        //  删除列表：遍历字典，仅包含不在 newIdSet 中的旧实体
        var deleteList = new List<TEntity>(originalDict.Count);
        foreach (var kvp in originalDict)
        {
            if (!newIdSet.Contains(kvp.Key))
            {
                deleteList.Add(kvp.Value);
            }
        }

        return new ListCompareResult<TEntity, TKey>(createList, updateList, deleteList);
    }
}
