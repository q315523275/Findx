using Findx.DependencyInjection;
using Findx.Extensions;
using System.Linq;
using System.Collections.Generic;
namespace Findx.Data
{
    public static class ScopedDictionaryExtensions
    {
        /// <summary>
        /// 获取连接串的UnitOfWork
        /// </summary>
        public static IUnitOfWork GetConnUnitOfWork(this ScopedDictionary dict, string connPrimary)
        {
            string key = $"UnitOfWork_ConnPrimary_{connPrimary}";
            return dict.TryGetValue<IUnitOfWork>(key, out var uow) ? uow : default;
        }

        /// <summary>
        /// 获取所有连接串的UnitOfWork
        /// </summary>
        public static IEnumerable<IUnitOfWork> GetConnUnitOfWorks(this ScopedDictionary dict)
        {
            return dict.Where(m => m.Key.StartsWith("UnitOfWork_ConnPrimary_")).Select(m => m.Value as IUnitOfWork);
        }

        /// <summary>
        /// 设置连接串的UnitOfWork
        /// </summary>
        public static void SetConnUnitOfWork(this ScopedDictionary dict, string connString, IUnitOfWork unitOfWork)
        {
            string key = $"UnitOfWork_ConnPrimary_{connString}";
            dict.TryAdd(key, unitOfWork);
        }
    }
}
