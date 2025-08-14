namespace Findx.Common;

/// <summary>
///     通过
/// </summary>
public class OkResponse : Response
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public OkResponse()
    {
    }
}

/// <summary>
///     泛型通过
/// </summary>
/// <typeparam name="T"></typeparam>
public class OkResponse<T> : Response<T>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="data">内容</param>
    public OkResponse(T data) : base(data)
    {
    }
}