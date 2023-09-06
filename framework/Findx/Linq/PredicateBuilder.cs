using System.Linq.Expressions;

namespace Findx.Linq;

/// <summary>
///     动态查询构建器
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    ///    创建一个新的表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ExpressionStarter<T> New<T>()
    {
        return new ExpressionStarter<T>();
    }
    
    /// <summary>
    ///     创建一个新的表达式
    /// </summary>
    /// <param name="expr"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ExpressionStarter<T> New<T>(Expression<Func<T, bool>> expr)
    {
        return new ExpressionStarter<T>(expr);
    }
}