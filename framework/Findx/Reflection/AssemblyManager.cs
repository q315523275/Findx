using System.Runtime.Loader;
using Findx.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace Findx.Reflection;

/// <summary>
///     程序集管理器
/// </summary>
public static class AssemblyManager
{
   
    private static Assembly[] _allAssemblies;
    private static Type[] _allTypes;

    static AssemblyManager()
    {
        Filters = ["System", "Microsoft", "netstandard", "dotnet", "Window", "mscorlib", "Newtonsoft", "Remotion.Linq"];
    }
    
    /// <summary>
    ///     过滤程序集名称集合
    /// </summary>
    public static string[] Filters { private get; set; }
    
    /// <summary>
    /// 获取 所有程序集
    /// </summary>
    public static Assembly[] AllAssemblies
    {
        get
        {
            if (_allAssemblies == null)
            {
                Initialize();
            }

            return _allAssemblies;
        }
    }

    /// <summary>
    /// 获取 所有类型
    /// </summary>
    public static Type[] AllTypes
    {
        get
        {
            if (_allTypes == null)
            {
                Initialize();
            }

            return _allTypes;
        }
    }

    /// <summary>
    ///     初始化
    /// </summary>
    public static void Initialize()
    {
        if (DependencyContext.Default != null)
        {
            _allAssemblies = DependencyContext.Default
                                              .GetDefaultAssemblyNames()
                                              .Where(x => x.Name != null && !Filters.Any(m => x.Name.StartsWith(m)))
                                              .Select(Assembly.Load).ToArray();
            _allTypes = _allAssemblies.SelectMany(m => m.GetTypes()).ToArray();
        }
        else
        {
            _allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                                    .Where(x => x.FullName != null && !Filters.Any(m => x.FullName.StartsWith(m)))
                                                    .ToArray();
            _allTypes = _allAssemblies.SelectMany(m => m.GetTypes()).ToArray();
            
            // Console.WriteLine("2:" + _allAssemblies.Select(x => x.FullName).JoinAsString("/"));
            //
            // var s3 = AssemblyLoadContext.Default.Assemblies.Where(x => x.FullName != null && !Filters.Any(m => x.FullName.StartsWith(m))).ToArray();
            // Console.WriteLine("3:" + s3.Select(x => x.FullName).JoinAsString("/"));
            //
            // var entryAssembly = Assembly.GetEntryAssembly();
            // var referencedAssemblies = entryAssembly?.GetReferencedAssemblies();
            // var s4 = referencedAssemblies?.Where(x => !Filters.Any(m => x.FullName.StartsWith(m)));
            // Console.WriteLine("4:" + s4?.Select(x => x.FullName).JoinAsString("/"));
        }
    }

    /// <summary>
    ///     查找指定条件的类型
    /// </summary>
    public static Type[] FindTypes(Func<Type, bool> predicate)
    {
        return AllTypes.Where(predicate).ToArray();
    }

    /// <summary>
    ///     查找指定基类的实现类型
    /// </summary>
    public static Type[] FindTypesByBase<TBaseType>()
    {
        var baseType = typeof(TBaseType);
        return FindTypesByBase(baseType);
    }

    /// <summary>
    ///     查找指定基类的实现类型
    /// </summary>
    public static Type[] FindTypesByBase(Type baseType)
    {
        return AllTypes.Where(type => type.IsDeriveClassFrom(baseType)).Distinct().ToArray();
    }

    /// <summary>
    ///     查找指定Attribute特性的实现类型
    /// </summary>
    public static Type[] FindTypesByAttribute<TAttribute>(bool inherit = true)
    {
        var attributeType = typeof(TAttribute);
        return FindTypesByAttribute(attributeType, inherit);
    }

    /// <summary>
    ///     查找指定Attribute特性的实现类型
    /// </summary>
    public static Type[] FindTypesByAttribute(Type attributeType, bool inherit = true)
    {
        return AllTypes.Where(type => type.IsDefined(attributeType, inherit)).Distinct().ToArray();
    }
}