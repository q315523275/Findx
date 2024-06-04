using Findx.Extensions;

namespace Findx.Data;

/// <summary>
///     作用域词典
/// </summary>
public static class ScopedDictionaryExtensions
{
    /// <summary>
    ///     获取连接串的UnitOfWork
    /// </summary>
    public static IUnitOfWork GetConnUnitOfWork(this ScopedDictionary dict, string connPrimary)
    {
        return dict.TryGetValue<IUnitOfWork>($"UnitOfWork_ConnPrimary_{connPrimary}", out var uow) ? uow : default;
    }

    /// <summary>
    ///     获取所有连接串的UnitOfWork
    /// </summary>
    public static IEnumerable<IUnitOfWork> GetAllConnUnitOfWork(this ScopedDictionary dict)
    {
        return dict.Where(m => m.Key.StartsWith("UnitOfWork_ConnPrimary_")).Select(m => m.Value as IUnitOfWork);
    }

    /// <summary>
    ///     设置连接串的UnitOfWork
    /// </summary>
    public static void SetConnUnitOfWork(this ScopedDictionary dict, string connString, IUnitOfWork unitOfWork)
    {
        dict.TryAdd($"UnitOfWork_ConnPrimary_{connString}", unitOfWork);
    }

    /// <summary>
    ///     获取指定实体类的UnitOfWork
    /// </summary>
    public static IUnitOfWork GetEntityUnitOfWork(this ScopedDictionary dict, Type entityType)
    {
        var key = $"UnitOfWork_EntityType_{entityType.FullName}";
        return dict.TryGetValue<IUnitOfWork>(key, out var uow) ? uow : default;
    }

    /// <summary>
    ///     获取所有实体类的UnitOfWork
    /// </summary>
    public static IEnumerable<IUnitOfWork> GetAllEntityUnitOfWork(this ScopedDictionary dict)
    {
        return dict.Where(m => m.Key.StartsWith("UnitOfWork_EntityType_")).Select(m => m.Value as IUnitOfWork);
    }

    /// <summary>
    ///     设置指定实体类的UnitOfWork
    /// </summary>
    public static void SetEntityUnitOfWork(this ScopedDictionary dict, Type entityType, IUnitOfWork unitOfWork)
    {
        var key = $"UnitOfWork_EntityType_{entityType.FullName}";
        dict.TryAdd(key, unitOfWork);
    }
}