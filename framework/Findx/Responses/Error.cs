namespace Findx.Responses;

/// <summary>
///     错误
/// </summary>
public class Error
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="code"></param>
    public Error(string message, string code)
    {
        Message = message;
        Code = code;
    }

    /// <summary>
    ///     错误号
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    ///     错误消息
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Message;
    }
}