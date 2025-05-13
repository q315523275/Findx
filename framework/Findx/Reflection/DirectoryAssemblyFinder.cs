using System.Runtime.Loader;

namespace Findx.Reflection;

/// <summary>
///     目录程序集查找器
/// </summary>
public class DirectoryAssemblyFinder : IAssemblyFinder
{
    private static readonly Dictionary<string, IEnumerable<Assembly>> CacheDict = new();

    private readonly string _path;

    /// <summary>
    ///     初始化一个<see cref="DirectoryAssemblyFinder" />类型的新实例
    /// </summary>
    public DirectoryAssemblyFinder(string path)
    {
        _path = path;
    }

    /// <summary>
    ///     查找指定条件的项
    /// </summary>
    /// <param name="predicate">筛选条件</param>
    /// <param name="fromCache">是否来自缓存</param>
    /// <returns></returns>
    public IEnumerable<Assembly> Find(Func<Assembly, bool> predicate, bool fromCache = false)
    {
        return FindAll(fromCache).Where(predicate);
    }

    /// <summary>
    ///     查找所有项
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Assembly> FindAll(bool fromCache = false)
    {
        if (fromCache && CacheDict.TryGetValue(_path, out var all)) return all;
        
        var files = Directory.EnumerateFiles(_path, "*.*", SearchOption.TopDirectoryOnly)
                             .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe"));
        
        var assemblies = files.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).Distinct();
        
        CacheDict[_path] = assemblies;
        
        return assemblies;
    }
}