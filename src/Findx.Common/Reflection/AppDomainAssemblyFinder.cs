using Findx.Extensions;
using Findx.Finders;
using Microsoft.Extensions.DependencyModel;

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
        /// 待过滤类型
        /// </summary>
        private readonly string[] _filter = { "System", "Microsoft", "netstandard", "dotnet", "Window", "mscorlib", "Newtonsoft", "Remotion.Linq" };

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
            // Core 中获取依赖对象的方式
            var context = DependencyContext.Default;
            if (context != null)
            {
                var lt = context.GetDefaultAssemblyNames()
                                .Where(x => x.Name != null && !_filter.Any(m => x.Name.StartsWith(m, StringComparison.OrdinalIgnoreCase)))
                                .Select(Assembly.Load);
                return lt;
            }

            // 传统方式
            var pathBase = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(pathBase, "*.dll", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(pathBase, ".exe", SearchOption.TopDirectoryOnly));

            return files.WhereIf(_filterSystemAssembly, name => !_filter.Any(n => name.StartsWith(n, StringComparison.OrdinalIgnoreCase))).Distinct().Select(Assembly.LoadFrom);
        }
    }
}
