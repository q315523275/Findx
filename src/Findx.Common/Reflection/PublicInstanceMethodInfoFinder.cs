using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
namespace Findx.Reflection
{
    /// <summary>
    /// 公共实例方法查找器
    /// </summary>
    public class PublicInstanceMethodInfoFinder : IMethodInfoFinder
    {
        /// <summary>
        /// 查找指定条件的项
        /// </summary>
        /// <param name="type">要查找的类型</param>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> Find(Type type, Func<MethodInfo, bool> predicate)
        {
            return FindAll(type).Where(predicate);
        }

        /// <summary>
        /// 查找所有项
        /// </summary>
        /// <param name="type">要查找的类型</param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> FindAll(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
