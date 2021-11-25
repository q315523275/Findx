using System;
using System.Linq;
using System.Linq.Expressions;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - IQueryable
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="skipCount">跳过数</param>
        /// <param name="takeCount">取数</param>
        /// <returns></returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int takeCount)
        {
            Check.NotNull(query, nameof(query));

            return query.Skip(skipCount).Take(takeCount);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQueryable"></typeparam>
        /// <param name="query"></param>
        /// <param name="skipCount">跳过数</param>
        /// <param name="takeCount">取数</param>
        /// <returns></returns>
        public static TQueryable PageBy<T, TQueryable>(this TQueryable query, int skipCount, int takeCount) where TQueryable : IQueryable<T>
        {
            Check.NotNull(query, nameof(query));

            return (TQueryable)query.Skip(skipCount).Take(takeCount);
        }

        /// <summary>
        /// 条件通过则进行谓词过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            Check.NotNull(query, nameof(query));

            return condition ? query.Where(predicate) : query;
        }

        /// <summary>
        /// 条件通过则进行谓词过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQueryable"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, bool>> predicate) where TQueryable : IQueryable<T>
        {
            Check.NotNull(query, nameof(query));

            return condition ? (TQueryable)query.Where(predicate) : query;
        }

        /// <summary>
        /// 条件通过则进行谓词过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate)
        {
            Check.NotNull(query, nameof(query));

            return condition ? query.Where(predicate) : query;
        }

        /// <summary>
        /// 条件通过则进行谓词过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQueryable"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, int, bool>> predicate) where TQueryable : IQueryable<T>
        {
            Check.NotNull(query, nameof(query));

            return condition ? (TQueryable)query.Where(predicate) : query;
        }
    }
}
