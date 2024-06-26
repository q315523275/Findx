﻿using System.Linq.Expressions;
using Findx.Common;

namespace Findx.Extensions;

/// <summary>
///     Expression表达式扩展操作类
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     以特定的条件运行组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="first">第一个Expression表达式</param>
    /// <param name="second">要组合的Expression表达式</param>
    /// <param name="merge">组合条件运算方式</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
        Check.NotNull(first, nameof(first));
        Check.NotNull(second, nameof(second));
        Check.NotNull(merge, nameof(merge));

        var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
        var secondBody = RebindParameterVisitor.ReplaceParameters(map, second.Body);
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    /// <summary>
    ///     以 Expression.AndAlso 组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="first">第一个Expression表达式</param>
    /// <param name="second">要组合的Expression表达式</param>
    /// <param name="ifExp">判断条件表达式，当此条件为true时，才执行组合</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> first, bool ifExp, Expression<Func<T, bool>> second)
    {
        Check.NotNull(first, nameof(first));
        Check.NotNull(second, nameof(second));
        return ifExp ? first.Compose(second, Expression.AndAlso) : first;
    }

    /// <summary>
    ///     以 Expression.AndAlso 组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="first">第一个Expression表达式</param>
    /// <param name="second">要组合的Expression表达式</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        Check.NotNull(first, nameof(first));
        Check.NotNull(second, nameof(second));
        return first.Compose(second, Expression.AndAlso);
    }

    /// <summary>
    ///     以 Expression.OrElse 组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="first">第一个Expression表达式</param>
    /// <param name="second">要组合的Expression表达式</param>
    /// <param name="ifExp">判断条件表达式，当此条件为true时，才执行组合</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> first, bool ifExp, Expression<Func<T, bool>> second)
    {
        Check.NotNull(first, nameof(first));
        Check.NotNull(second, nameof(second));
        return ifExp ? first.Compose(second, Expression.OrElse) : first;
    }

    /// <summary>
    ///     以 Expression.OrElse 组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="first">第一个Expression表达式</param>
    /// <param name="second">要组合的Expression表达式</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        Check.NotNull(first, nameof(first));
        Check.NotNull(second, nameof(second));
        return first.Compose(second, Expression.OrElse);
    }

    /// <summary>
    ///     RebindParameterVisitor
    /// </summary>
    private class RebindParameterVisitor : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        private RebindParameterVisitor(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new RebindParameterVisitor(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_map.TryGetValue(node, out var replacement)) node = replacement;
            return base.VisitParameter(node);
        }
    }
}