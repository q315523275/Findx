using System.Linq.Expressions;

namespace Findx.Linq;

/// <summary>
///     Expression表达式扩展类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Expressionable<T> where T : class, new()
{
    private Expression<Func<T, bool>> _exp;

    /// <summary>
    ///     And
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public Expressionable<T> And(Expression<Func<T, bool>> exp)
    {
        if (_exp == null)
            _exp = exp;
        else
            _exp = _exp.And(exp);

        return this;
    }

    /// <summary>
    ///     AndIF
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    public Expressionable<T> AndIF(bool isExp, Expression<Func<T, bool>> exp)
    {
        if (isExp) And(exp);

        return this;
    }

    /// <summary>
    ///     Or
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public Expressionable<T> Or(Expression<Func<T, bool>> exp)
    {
        if (_exp == null)
            _exp = exp;
        else
            _exp = _exp.Or(exp);

        return this;
    }

    /// <summary>
    ///     OrIF
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    public Expressionable<T> OrIF(bool isExp, Expression<Func<T, bool>> exp)
    {
        if (isExp) Or(exp);

        return this;
    }

    /// <summary>
    ///     转换为表达式
    /// </summary>
    /// <returns></returns>
    public Expression<Func<T, bool>> ToExpression()
    {
        if (_exp == null) _exp = it => true;

        return _exp;
    }
}