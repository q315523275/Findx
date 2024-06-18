namespace Findx.Linq;

/// <summary>
///     数据排序构建器
/// </summary>
public static class SortConditionBuilder
{
    /// <summary>
    ///     创建排序表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ExpressionSorter<T> New<T>()
    {
        return new ExpressionSorter<T>();
    }
}