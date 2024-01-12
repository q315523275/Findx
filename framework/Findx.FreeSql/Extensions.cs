using System.Collections.Generic;
using FreeSql;

namespace Findx.FreeSql;

/// <summary>
///     扩展
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     记录总数
    /// </summary>
    /// <param name="select"></param>
    /// <param name="condition"></param>
    /// <param name="count"></param>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static ISelect<T1> CountIf<T1>(this ISelect<T1> select, bool condition, out long count) where T1 : class
    {
        if (condition) return select.Count(out count);

        count = 0;
        return select;
    }

    /// <summary>
    ///     记录总数
    /// </summary>
    /// <param name="select"></param>
    /// <param name="condition"></param>
    /// <param name="count"></param>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <returns></returns>
    public static ISelect<T1, T2> CountIf<T1, T2>(this ISelect<T1, T2> select, bool condition, out long count)
        where T1 : class where T2 : class
    {
        if (condition) return select.Count(out count);

        count = 0;
        return select;
    }

    /// <summary>
    ///     记录总数
    /// </summary>
    /// <param name="select"></param>
    /// <param name="condition"></param>
    /// <param name="count"></param>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <returns></returns>
    public static ISelect<T1, T2, T3> CountIf<T1, T2, T3>(this ISelect<T1, T2, T3> select, bool condition,
        out long count)
        where T1 : class where T2 : class where T3 : class
    {
        if (condition) return select.Count(out count);

        count = 0;
        return select;
    }

    /// <summary>
    ///     记录总数
    /// </summary>
    /// <param name="select"></param>
    /// <param name="condition"></param>
    /// <param name="count"></param>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4> CountIf<T1, T2, T3, T4>(this ISelect<T1, T2, T3, T4> select, bool condition,
        out long count)
        where T1 : class where T2 : class where T3 : class where T4 : class
    {
        if (condition) return select.Count(out count);

        count = 0;
        return select;
    }

    /// <summary>
    ///     记录总数
    /// </summary>
    /// <param name="select"></param>
    /// <param name="condition"></param>
    /// <param name="count"></param>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5> CountIf<T1, T2, T3, T4, T5>(this ISelect<T1, T2, T3, T4, T5> select,
        bool condition, out long count)
        where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class
    {
        if (condition) return select.Count(out count);

        count = 0;
        return select;
    }
}