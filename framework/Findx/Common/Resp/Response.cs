namespace Findx.Common;

/// <summary>
///     响应基类
/// </summary>
public abstract class Response
{
    /// <summary>
    ///     Ctor
    /// </summary>
    protected Response()
    {
        Errors = [];
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="errors">异常集合</param>
    protected Response(List<Error> errors)
    {
        Errors = errors ?? [];
    }

    /// <summary>
    ///     异常集合
    /// </summary>
    public List<Error> Errors { get; }

    /// <summary>
    ///     是否异常
    /// </summary>
    public bool IsError => Errors.Count > 0;
}

/// <summary>
///     响应
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Response<T> : Response
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="data">内容</param>
    protected Response(T data)
    {
        Data = data;
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="errors">异常集合</param>
    protected Response(List<Error> errors) : base(errors)
    {
    }

    /// <summary>
    ///     内容
    /// </summary>
    public T Data { get; private set; }
}