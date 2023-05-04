namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 集合
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     在集合指定的位置开始插入集合数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="index"></param>
    /// <param name="items"></param>
    public static void InsertRange<T>(this IList<T> source, int index, IEnumerable<T> items)
    {
        foreach (var item in items) source.Insert(index++, item);
    }

    /// <summary>
    ///     查找集合索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static int FindIndex<T>(this IList<T> source, Predicate<T> selector)
    {
        for (var i = 0; i < source.Count; ++i)
            if (selector(source[i]))
                return i;
        return -1;
    }

    /// <summary>
    ///     集合头部插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="item"></param>
    public static void AddFirst<T>(this IList<T> source, T item)
    {
        source.Insert(0, item);
    }

    /// <summary>
    ///     集合尾部插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="item"></param>
    public static void AddLast<T>(this IList<T> source, T item)
    {
        source.Insert(source.Count, item);
    }

    /// <summary>
    ///     集合指定对象之后插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="existingItem"></param>
    /// <param name="item"></param>
    public static void InsertAfter<T>(this IList<T> source, T existingItem, T item)
    {
        var index = source.IndexOf(existingItem);
        if (index < 0)
        {
            source.AddFirst(item);
            return;
        }

        source.Insert(index + 1, item);
    }

    /// <summary>
    ///     集合自定义查询对象之后插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="item"></param>
    public static void InsertAfter<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        var index = source.FindIndex(selector);
        if (index < 0)
        {
            source.AddFirst(item);
            return;
        }

        source.Insert(index + 1, item);
    }

    /// <summary>
    ///     集合指定对象之前插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="existingItem"></param>
    /// <param name="item"></param>
    public static void InsertBefore<T>(this IList<T> source, T existingItem, T item)
    {
        var index = source.IndexOf(existingItem);
        if (index < 0)
        {
            source.AddLast(item);
            return;
        }

        source.Insert(index, item);
    }

    /// <summary>
    ///     集合自定义查询对象之前插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="item"></param>
    public static void InsertBefore<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        var index = source.FindIndex(selector);
        if (index < 0)
        {
            source.AddLast(item);
            return;
        }

        source.Insert(index, item);
    }

    /// <summary>
    ///     集合对象替换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="item"></param>
    public static void ReplaceWhile<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        for (var i = 0; i < source.Count; i++)
            if (selector(source[i]))
                source[i] = item;
    }

    /// <summary>
    ///     集合满足查询条件对象替换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="itemFactory"></param>
    public static void ReplaceWhile<T>(this IList<T> source, Predicate<T> selector, Func<T, T> itemFactory)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (selector(item)) source[i] = itemFactory(item);
        }
    }

    /// <summary>
    ///     集合内替换满足查询条件第一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="item"></param>
    public static void ReplaceOne<T>(this IList<T> source, Predicate<T> selector, T item)
    {
        for (var i = 0; i < source.Count; i++)
            if (selector(source[i]))
            {
                source[i] = item;
                return;
            }
    }

    /// <summary>
    ///     集合内替换满足查询条件的第一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="itemFactory"></param>
    public static void ReplaceOne<T>(this IList<T> source, Predicate<T> selector, Func<T, T> itemFactory)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (selector(item))
            {
                source[i] = itemFactory(item);
                return;
            }
        }
    }

    /// <summary>
    ///     集合内替换指定的第一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="item"></param>
    /// <param name="replaceWith"></param>
    public static void ReplaceOne<T>(this IList<T> source, T item, T replaceWith)
    {
        for (var i = 0; i < source.Count; i++)
            if (Comparer<T>.Default.Compare(source[i], item) == 0)
            {
                source[i] = replaceWith;
                return;
            }
    }

    /// <summary>
    ///     移动集合数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="targetIndex"></param>
    public static void MoveItem<T>(this List<T> source, Predicate<T> selector, int targetIndex)
    {
        if (!targetIndex.IsBetween(0, source.Count - 1))
            throw new IndexOutOfRangeException("targetIndex should be between 0 and " + (source.Count - 1));

        var currentIndex = source.FindIndex(0, selector);
        if (currentIndex == targetIndex) return;

        var item = source[currentIndex];
        source.RemoveAt(currentIndex);
        source.Insert(targetIndex, item);
    }

    /// <summary>
    ///     获取集合指定条件数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static T GetOrAdd<T>(this IList<T> source, Func<T, bool> selector, Func<T> factory)
    {
        Check.NotNull(source, nameof(source));

        var item = source.FirstOrDefault(selector);

        if (item == null)
        {
            item = factory();
            source.Add(item);
        }

        return item;
    }

    /// <summary>
    ///     Sort a list by a topological sorting, which consider their dependencies.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="source">A list of objects to sort</param>
    /// <param name="getDependencies">Function to resolve the dependencies</param>
    /// <returns>
    ///     Returns a new list ordered by dependencies.
    ///     If A depends on B, then B will come before than A in the resulting list.
    /// </returns>
    public static List<T> SortByDependencies<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies)
    {
        /* See: http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
         *      http://en.wikipedia.org/wiki/Topological_sorting
         */

        var sorted = new List<T>();
        var visited = new Dictionary<T, bool>();

        foreach (var item in source) SortByDependenciesVisit(item, getDependencies, sorted, visited);

        return sorted;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="item">Item to resolve</param>
    /// <param name="getDependencies">Function to resolve the dependencies</param>
    /// <param name="sorted">List with the sortet items</param>
    /// <param name="visited">Dictionary with the visited items</param>
    private static void SortByDependenciesVisit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted,
        Dictionary<T, bool> visited)
    {
        var alreadyVisited = visited.TryGetValue(item, out var inProcess);

        if (alreadyVisited)
        {
            if (inProcess) throw new ArgumentException("Cyclic dependency found! Item: " + item);
        }
        else
        {
            visited[item] = true;

            var dependencies = getDependencies(item);
            if (dependencies != null)
                foreach (var dependency in dependencies)
                    SortByDependenciesVisit(dependency, getDependencies, sorted, visited);

            visited[item] = false;
            sorted.Add(item);
        }
    }
}