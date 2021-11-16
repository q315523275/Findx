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

        //private list<assembly> _assemblies = new list<assembly>();
        //public assembly[] findallassembly(bool filterassembly = true)
        //{
        //    var filter = new string[]{
        //        "system",
        //        "microsoft",
        //        "netstandard",
        //        "dotnet",
        //        "window",
        //        "mscorlib",
        //        "newtonsoft",
        //        "remotion.linq"
        //    };
        //    // core中获取依赖对象的方式
        //    dependencycontext context = dependencycontext.default;
        //    if (context != null)
        //    {
        //        list<string> names = new list<string>();
        //        string[] dllnames = context.compilelibraries.selectmany(m => m.assemblies).distinct().select(m => m.replace(".dll", "")).toarray();
        //        if (dllnames.length > 0)
        //        {
        //            names = (from name in dllnames
        //                     let index = name.lastindexof('/') + 1
        //                     select name.substring(index))
        //                    .distinct()
        //                    .whereif(filterassembly, name => !filter.any(name.startswith))
        //                    .tolist();
        //        }
        //        return loadfromfiles(names);
        //    }
        //    // 传统方式
        //    string pathbase = appdomain.currentdomain.basedirectory;
        //    string[] files = directory.getfiles(pathbase, "*.dll", searchoption.topdirectoryonly)
        //                              .concat(directory.getfiles(pathbase, ".exe", searchoption.topdirectoryonly))
        //                              .toarray();
        //    return files.whereif(filterassembly, f => !filter.any(n => f.startswith(n, stringcomparison.ordinalignorecase))).distinct().toarray().select(assembly.loadfrom).tolist().toarray();
        //}


        /// <summary>
        /// 从文件加载程序集对象
        /// </summary>
        /// <param name="files">文件(名称集合)</param>
        /// <returns></returns>
        private static Assembly[] LoadFromFiles(List<string> files)
        {
            List<Assembly> assemblies = new List<Assembly>();
            files?.ToList().ForEach(f =>
            {
                AssemblyName name = new AssemblyName(f);
                try
                {
                    assemblies.Add(Assembly.Load(name));
                }
                catch { }
            });
            return assemblies.ToArray();
        }
        /// <summary>
        /// 查找所有程序集
        /// </summary>
        /// <returns></returns>
        protected override Assembly[] FindAllItems()
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

            // Core中获取依赖对象的方式
            DependencyContext context = DependencyContext.Default;
            if (context != null)
            {
                List<string> names = new List<string>();
                string[] dllNames = context.CompileLibraries.SelectMany(m => m.Assemblies).Distinct().Select(m => m.Replace(".dll", "")).ToArray();
                if (dllNames.Length > 0)
                {
                    names = (from name in dllNames
                             let index = name.LastIndexOf('/') + 1
                             select name.Substring(index))
                            .Distinct()
                            .WhereIf(_filterSystemAssembly, name => !filter.Any(n => name.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                    return LoadFromFiles(names);
                }
            }

            // 传统方式
            string pathbase = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(pathbase, "*.dll", SearchOption.TopDirectoryOnly)
                                      .Concat(Directory.GetFiles(pathbase, ".exe", SearchOption.TopDirectoryOnly))
                                      .ToArray();
            return files.WhereIf(_filterSystemAssembly, name => !filter.Any(n => name.StartsWith(n, StringComparison.OrdinalIgnoreCase))).Distinct().Select(Assembly.LoadFrom).ToArray();
        }
    }
}
