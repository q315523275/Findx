using System.Linq.Expressions;
using Findx.Extensions;

namespace Findx.Linq;

/// <summary>
/// ExpressionStarter
/// </summary>
/// <typeparam name="T"></typeparam>
public class ExpressionStarter<T>
{
    private Expression<Func<T, bool>> _predicate;

    internal ExpressionStarter()
    {
    }

    internal ExpressionStarter(Expression<Func<T, bool>> exp)
    {
        _predicate = exp;
    }
    
    /// <summary>
    ///     And
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public ExpressionStarter<T> And(Expression<Func<T, bool>> predicate)
    {
        _predicate = _predicate == null ? predicate : _predicate.And(predicate);
        return this;
    }

    /// <summary>
    ///     And If
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public ExpressionStarter<T> AndIf(bool isExp, Expression<Func<T, bool>> predicate)
    {
        if (isExp) And(predicate);
        return this;
    }
    
    /// <summary>
    ///     Or
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public ExpressionStarter<T> Or(Expression<Func<T, bool>> predicate)
    {
        _predicate = _predicate == null ? predicate : _predicate.Or(predicate);
        return this;
    }
    
    /// <summary>
    ///     Or If
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public ExpressionStarter<T> OrIf(bool isExp, Expression<Func<T, bool>> predicate)
    {
        if (isExp) Or(predicate);
        return this;
    }
    
    /// <summary>
    ///    构建表达式
    /// </summary>
    /// <returns></returns>
    public Expression<Func<T, bool>> Build()
    {
        return _predicate;
    }
}