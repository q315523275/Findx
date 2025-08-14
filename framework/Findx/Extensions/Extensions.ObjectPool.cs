using Findx.Common;
using Findx.Common.DisposableEx;
using Microsoft.Extensions.ObjectPool;

namespace Findx.Extensions;

/// <summary>
///     对象池对接
/// </summary>
public partial class Extensions
{
    /// <summary>
    ///    获取池中对象
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="pooledObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IDisposable Get<T>(this ObjectPool<T> pool, out T pooledObject) where T : class
    {
        var rented = pool.Get();
        pooledObject = rented;
        return new ActionDisposable(() => pool.Return(rented));
    }
    
    /// <summary>
    ///     获取池中对象
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="pooledObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IDisposable Get<T>(this IPool<T> pool, out T pooledObject) where T : class
    {
        var rented = pool.Rent();
        pooledObject = rented;
        return new ActionDisposable(() => pool.Return(rented));
    }
}