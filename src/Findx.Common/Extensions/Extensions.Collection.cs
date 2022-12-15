using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 集合
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 集合是否为NULL或者长度为0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        /// 将一个项目添加到该集合中（如果该项目不在集合中）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryAdd<T>(this ICollection<T> source, T item)
        {
            Check.NotNull(source, nameof(source));

            if (source.Contains(item))
            {
                return false;
            }

            source.Add(item);
            return true;
        }

        /// <summary>
        /// 将一个集合添加到该集合中(如果集合里的项目不在集合中)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">原集合</param>
        /// <param name="items">待合并集合</param>
        /// <returns>已合并的集合项</returns>
        public static IEnumerable<T> TryAdd<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            Check.NotNull(source, nameof(source));

            var addedItems = new List<T>();

            foreach (var item in items)
            {
                if (source.Contains(item))
                {
                    continue;
                }

                source.Add(item);
                addedItems.Add(item);
            }

            return addedItems;
        }

        /// <summary>
        /// 根据给定的<paramref name ="predicate" />，将一个项目添加到该集合中（如果该项目不在集合中）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="itemFactory"></param>
        /// <returns></returns>
        public static bool TryAdd<T>(this ICollection<T> source, Func<T, bool> predicate, Func<T> itemFactory)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));
            Check.NotNull(itemFactory, nameof(itemFactory));

            if (source.Any(predicate))
            {
                return false;
            }

            source.Add(itemFactory());
            return true;
        }

        /// <summary>
        /// 从集合中删除所有满足给定<paramref name ="predicate" />的项目
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> RemoveAll<T>(this ICollection<T> source, Func<T, bool> predicate)
        {
            var items = source.Where(predicate);

            foreach (var item in items)
            {
                source.Remove(item);
            }

            return items;
        }
        
        /// <summary>
        /// 向集合中添加指定项
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ifExp">判断条件表达式，当此条件为true时，才进行添加</param>
        /// <param name="item">泛型集合项</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AddIf<T>(this ICollection<T> source, bool ifExp, T item)
        {
            if (ifExp) source.Add(item);
            return source;
        }
    }
}
