namespace Findx.Linq;

/// <summary>
///     Expression表达式构建类
/// </summary>
public class ExpressionBuilder
{
    /// <summary>
    ///     创建自定义Expression表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Expressionable<T> Create<T>() where T : class, new()
    {
        return new Expressionable<T>();
    }

    /// <summary>
    ///     创建排序表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Expressionsort<T> CreateOrder<T>() where T : class, new()
    {
        return new Expressionsort<T>();
    }
}