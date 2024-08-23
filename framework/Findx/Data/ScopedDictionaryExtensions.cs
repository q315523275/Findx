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
    public static IUnitOfWork GetUnitOfWork(this ScopedDictionary dict, string primary)
    {
        return dict.TryGetValue<IUnitOfWork>($"UnitOfWork_Primary_{primary}", out var uow) ? uow : default;
    }

    /// <summary>
    ///     设置连接串的UnitOfWork
    /// </summary>
    public static void SetUnitOfWork(this ScopedDictionary dict, string primary, IUnitOfWork unitOfWork)
    {
        dict.TryAdd($"UnitOfWork_Primary_{primary}", unitOfWork);
    }
    
    /// <summary>
    ///     获取所有连接串的UnitOfWork
    /// </summary>
    public static IEnumerable<IUnitOfWork> GetAllUnitOfWork(this ScopedDictionary dict)
    {
        return dict.Where(m => m.Key.StartsWith("UnitOfWork_Primary_")).Select(m => m.Value as IUnitOfWork);
    }
}