using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Data;
using FreeSql;
using FreeSql.Extensions.EntityUtil;

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
        count = -1;
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
    public static ISelect<T1, T2> CountIf<T1, T2>(this ISelect<T1, T2> select, bool condition, out long count) where T1 : class where T2 : class
    {
        if (condition) return select.Count(out count);
        count = -1;
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
    public static ISelect<T1, T2, T3> CountIf<T1, T2, T3>(this ISelect<T1, T2, T3> select, bool condition, out long count) where T1 : class where T2 : class where T3 : class
    {
        if (condition) return select.Count(out count);
        count = -1;
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
    public static ISelect<T1, T2, T3, T4> CountIf<T1, T2, T3, T4>(this ISelect<T1, T2, T3, T4> select, bool condition, out long count) where T1 : class where T2 : class where T3 : class where T4 : class
    {
        if (condition) return select.Count(out count);
        count = -1;
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
    public static ISelect<T1, T2, T3, T4, T5> CountIf<T1, T2, T3, T4, T5>(this ISelect<T1, T2, T3, T4, T5> select, bool condition, out long count) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class
    {
        if (condition) return select.Count(out count);
        count = -1;
        return select;
    }

    /// <summary>
    ///     比较实体，计算出值发生变化的属性，以及属性变化的前后值
    /// </summary>
    /// <param name="fsql"></param>
    /// <param name="newData">最新的实体对象</param>
    /// <param name="oldData">原始的实体对象</param>
    /// <returns>key: 属性名, value: [新值, 旧值]</returns>
    public static Dictionary<string, object[]> CompareState<TEntity>(this IFreeSql fsql, TEntity newData, TEntity oldData) where TEntity : IEntity
    {
        if (newData == null)
            return null;

        var entityType = typeof(TEntity);

        var table = fsql.CodeFirst.GetTableByEntity(entityType);
        if (table.Primarys.Any() == false)
            throw new Exception($"实体{table.CsName}必须存在主键配置");

        var key = fsql.GetEntityKeyString(entityType, newData, false);
        if (string.IsNullOrEmpty(key))
            throw new Exception($"实体{table.CsName}的主键值不可为空");

        var res = fsql.CompareEntityValueReturnColumns(entityType, oldData, newData, false).ToDictionary(a => a, a =>
                    table.Columns.TryGetValue(a, out var columnInfo) ? new[]
                        { 
                            fsql.GetEntityValueWithPropertyName(entityType, newData, columnInfo.CsName), 
                            fsql.GetEntityValueWithPropertyName(entityType, oldData, columnInfo.CsName)
                        }
                        : [null, null]);
        return res;
    }
    
    /// <summary>
    ///     比较实体，计算出值发生变化的属性，以及属性变化的值
    /// </summary>
    /// <param name="fsql">freeSql实例</param>
    /// <param name="newData">最新的实体对象</param>
    /// <param name="oldData">原始的实体对象</param>
    /// <returns>key: 属性名, value: 新值</returns>
    public static Dictionary<string, object> CompareChangeValues<TEntity>(this IFreeSql fsql, TEntity newData, TEntity oldData) where TEntity : IEntity
    {
        if (newData == null) 
            return null;
        
        var entityType = typeof(TEntity);
        
        var table = fsql.CodeFirst.GetTableByEntity(entityType);
        if (table.Primarys.Any() == false) 
            throw new Exception($"实体{table.CsName}必须存在主键配置");
        
        var key = fsql.GetEntityKeyString(entityType, newData, false);
        if (string.IsNullOrEmpty(key)) 
            throw new Exception($"实体{table.CsName}的主键值不可为空");

        var res = fsql.CompareEntityValueReturnColumns(entityType, oldData, newData, false).ToDictionary(a => a,
            a => table.Columns.TryGetValue(a, out var columnInfo) ? fsql.GetEntityValueWithPropertyName(entityType, newData, columnInfo.CsName) : null);

        return res;
    }
}