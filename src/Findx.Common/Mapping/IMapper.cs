namespace Findx.Mapping;

/// <summary>
///     对象映射器
/// </summary>
public interface IMapper
{
    /// <summary>
    ///     将对象映射为指定类型
    /// </summary>
    /// <typeparam name="TTarget">要映射的目标类型</typeparam>
    /// <param name="source">源对象</param>
    /// <returns>目标类型的对象</returns>
    TTarget MapTo<TTarget>(object source);

    /// <summary>
    ///     使用源类型的对象更新目标类型的对象
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <param name="source">源对象</param>
    /// <param name="target">待更新的目标对象</param>
    /// <returns>更新后的目标类型对象</returns>
    TTarget MapTo<TSource, TTarget>(TSource source, TTarget target);
}