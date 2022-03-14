using Findx.Extensions;
using Findx.Finders;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Findx.Reflection
{
    /// <summary>
    /// 应用程序域程序集查找器
    /// </summary>
    public class AppDomainAssemblyFinder : FinderBase<Assembly>, IAppDomainAssemblyFinder
    {
        /// <summary>
        /// 是否过滤系统程序集
        /// </summary>
        private readonly bool _filterSystemAssembly;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="filterSystemAssembly"></param>
        public AppDomainAssemblyFinder(bool filterSystemAssembly = true)
        {
            _filterSystemAssembly = filterSystemAssembly;
        }

        /// <summary>
        /// 从文件加载程序集对象
        /// </summary>
        /// <param name="files">文件(名称集合)</param>
        /// <returns></returns>
        private static IEnumerable<Assembly> LoadFromFiles(IEnumerable<string> files)
        {
            foreach (var f in files)
            {
                yield return Assembly.Load(new AssemblyName(f));
            }
        }

        /// <summary>
        /// 查找所有程序集
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Assembly> FindAllItems()
        {
            // 待过滤类型
            var filter = new string[]{
                "System",
                "Microsoft",
                "netstandard",
                "dotnet",
                "Window",
                "mscorlib",
                "Newtonsoft",
                "Remotion.Linq"
            };

            // Core 中获取依赖对象的方式
            DependencyContext context = DependencyContext.Default;
            if (context != null)
            {
                //var dllNames = context.CompileLibraries.SelectMany(m => m.Assemblies).Distinct().Select(m => m.Replace(".dll", ""));
                var dllNames = context.GetDefaultAssemblyNames().Distinct().Select(m => m.Name.Replace(".dll", ""));
                if (dllNames.Count() > 0)
                {
                    var names = (from name in dllNames
                                 let index = name.LastIndexOf('/') + 1
                                 select name.Substring(index))
                                 .Distinct()
                                 .WhereIf(_filterSystemAssembly, name => !filter.Any(n => name.StartsWith(n, StringComparison.OrdinalIgnoreCase)));

                    return LoadFromFiles(names);
                }
            }

            // 传统方式
            string pathbase = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(pathbase, "*.dll", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(pathbase, ".exe", SearchOption.TopDirectoryOnly));

            return files.WhereIf(_filterSystemAssembly, name => !filter.Any(n => name.StartsWith(n, StringComparison.OrdinalIgnoreCase))).Distinct().Select(Assembly.LoadFrom);
        }
    }
}
