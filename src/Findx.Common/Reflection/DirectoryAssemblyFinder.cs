﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Findx.Reflection
{
    /// <summary>
    /// 目录程序集查找器
    /// </summary>
    public class DirectoryAssemblyFinder : IAssemblyFinder
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<Assembly>> CacheDict = new ConcurrentDictionary<string, IEnumerable<Assembly>>();
        private readonly string _path;

        /// <summary>
        /// 初始化一个<see cref="DirectoryAssemblyFinder"/>类型的新实例
        /// </summary>
        public DirectoryAssemblyFinder(string path)
        {
            _path = path;
        }

        /// <summary>
        /// 查找指定条件的项
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <param name="fromCache">是否来自缓存</param>
        /// <returns></returns>
        public IEnumerable<Assembly> Find(Func<Assembly, bool> predicate, bool fromCache = false)
        {
            return FindAll(fromCache).Where(predicate);
        }

        /// <summary>
        /// 查找所有项
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Assembly> FindAll(bool fromCache = false)
        {
            if (fromCache && CacheDict.ContainsKey(_path))
            {
                return CacheDict[_path];
            }
            var files = Directory.GetFiles(_path, "*.dll", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(_path, "*.exe", SearchOption.TopDirectoryOnly));
            var assemblies = files.Select(Assembly.LoadFrom).Distinct();
            CacheDict[_path] = assemblies;
            return assemblies;
        }

    }
}
