namespace Findx.Responses;

/// <summary>
///     错误结果
/// </summary>
public class ErrorResponse : Response
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="error">错误信息</param>
    public ErrorResponse(Error error) : base([error])
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="errors">错误集合</param>
    public ErrorResponse(List<Error> errors) : base(errors)
    {
    }
}