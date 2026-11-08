using Findx.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace Findx.Reflection;

/// <summary>
///     程序集管理器
/// </summary>
public static class AssemblyManager
{
    private static readonly Lazy<Assembly[]> AllAssembliesLazy = new(LoadAllAssemblies, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<Type[]> AllTypesLazy = new(LoadAllTypes, LazyThreadSafetyMode.ExecutionAndPublication);

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
    public static Assembly[] AllAssemblies => AllAssembliesLazy.Value;

    /// <summary>
    /// 获取 所有类型
    /// </summary>
    public static Type[] AllTypes => AllTypesLazy.Value;

    /// <summary>
    ///     初始化（强制触发惰性扫描，并发安全）
    /// </summary>
    public static void Initialize()
    {
        _ = AllAssembliesLazy.Value;
        _ = AllTypesLazy.Value;
    }

    /// <summary>
    ///     加载所有程序集
    /// </summary>
    private static Assembly[] LoadAllAssemblies()
    {
        if (DependencyContext.Default != null)
        {
            return DependencyContext.Default
                                     .GetDefaultAssemblyNames()
                                     .Where(x => x.Name != null && !Filters.Any(m => x.Name.StartsWith(m)))
                                     .Select(Assembly.Load).ToArray();
        }

        return AppDomain.CurrentDomain.GetAssemblies()
                                     .Where(x => x.FullName != null && !Filters.Any(m => x.FullName.StartsWith(m)))
                                     .ToArray();
    }

    /// <summary>
    ///     加载所有类型（单个程序集类型加载失败时容错，避免启动崩溃）
    /// </summary>
    private static Type[] LoadAllTypes()
    {
        return AllAssembliesLazy.Value.SelectMany(SafeGetTypes).ToArray();
    }

    /// <summary>
    ///     安全获取程序集内类型：部分类型加载失败（缺依赖）时跳过失败项
    /// </summary>
    private static Type[] SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray();
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