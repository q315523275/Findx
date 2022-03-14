using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 集合
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 使用每个成员之间的指定分隔符，对集合的成员进行串联
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string JoinAsString(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 使用每个成员之间的指定分隔符，对集合的成员进行串联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 将集合展开并分别转换成字符串，再以指定的分隔符衔接，拼成一个字符串返回。默认分隔符为逗号
        /// </summary>
        /// <param name="collection"> 要处理的集合 </param>
        /// <param name="separator"> 分隔符，默认为逗号 </param>
        /// <returns> 拼接后的字符串 </returns>
        public static string ExpandAndToString<T>(this IEnumerable<T> collection, string separator = ",")
        {
            return collection.ExpandAndToString(t => t?.ToString(), separator);
        }

        /// <summary>
        /// 循环集合的每一项，调用委托生成字符串，返回合并后的字符串。默认分隔符为逗号
        /// </summary>
        /// <param name="collection">待处理的集合</param>
        /// <param name="itemFormatFunc">单个集合项的转换委托</param>
        /// <param name="separator">分隔符，默认为逗号</param>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns></returns>
        public static string ExpandAndToString<T>(this IEnumerable<T> collection, Func<T, string> itemFormatFunc, string separator = ",")
        {
            // collection = collection as IList<T> ?? collection.ToList();

            Check.NotNull(itemFormatFunc, nameof(itemFormatFunc));

            if (!collection.Any())
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            int i = 0;
            int count = collection.Count();
            foreach (T t in collection)
            {
                if (i == count - 1)
                {
                    sb.Append(itemFormatFunc(t));
                }
                else
                {
                    sb.Append(itemFormatFunc(t) + separator);
                }
                i++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 过滤集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 过滤集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, int, bool> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 集合是否为空
        /// </summary>
        /// <param name="source"> 要处理的集合 </param>
        /// <typeparam name="T"> 动态类型 </typeparam>
        /// <returns> 为空返回True，不为空返回False </returns>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            // source = source as IList<T> ?? source.ToList();
            return !source.Any();
        }

        /// <summary>
        /// 将字符串集合按指定前缀排序
        /// </summary>
        public static IEnumerable<T> OrderByPrefixes<T>(this IEnumerable<T> source, Func<T, string> keySelector, params string[] prefixes)
        {
            var all = source.OrderBy(keySelector).ToList();
            List<T> result = new List<T>();
            foreach (string prefix in prefixes)
            {
                List<T> tmpList = all.Where(m => keySelector(m).StartsWith(prefix)).OrderBy(keySelector).ToList();
                all = all.Except(tmpList).ToList();
                result.AddRange(tmpList);
            }
            result.AddRange(all);
            return result;
        }

        /// <summary>
        /// 根据指定条件返回集合中不重复的元素
        /// </summary>
        /// <typeparam name="T">动态类型</typeparam>
        /// <typeparam name="TKey">动态筛选条件类型</typeparam>
        /// <param name="source">要操作的源</param>
        /// <param name="keySelector">重复数据筛选条件</param>
        /// <returns>不重复元素的集合</returns>
        public static IEnumerable<T> DistinctBy2<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));

            // source = source as IList<T> ?? source.ToList();

            return source.GroupBy(keySelector).Select(group => group.First());
        }

        /// <summary>
        /// Concurrently Executes async actions for each item of <see cref="IEnumerable<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam>
        /// <param name="enumerable">instance of <see cref="IEnumerable<typeparamref name="T"/>"/></param>
        /// <param name="func">an async <see cref="Func{T, TResult}" /> to execute</param>
        /// <param name="maxActionsToRunInParallel">Optional, max numbers of the actions to run in parallel,
        /// Must be grater than 0</param>
        /// <returns>A Task representing an async operation</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the maxActionsToRunInParallel is less than 1</exception>
        public static async Task ForEachAsyncConcurrent<T>(this IEnumerable<T> enumerable, Func<T, Task> func, int? maxActionsToRunInParallel = null)
        {
            if (maxActionsToRunInParallel.HasValue)
            {
                using (var semaphoreSlim = new SemaphoreSlim(maxActionsToRunInParallel.Value, maxActionsToRunInParallel.Value))
                {
                    var tasksWithThrottler = new List<Task>();

                    foreach (var item in enumerable)
                    {
                        // Increment the number of currently running tasks and wait if they are more than limit.
                        await semaphoreSlim.WaitAsync();

                        tasksWithThrottler.Add(Task.Run(async () =>
                        {
                            await func(item).ContinueWith(res =>
                            {
                                // action is completed, so decrement the number of currently running tasks
                                semaphoreSlim.Release();
                            });
                        }));
                    }

                    // Wait for all of the provided tasks to complete.
                    await Task.WhenAll(tasksWithThrottler);
                }
            }
            else
            {
                await Task.WhenAll(enumerable.Select(item => func(item)));
            }
        }
    }
}
