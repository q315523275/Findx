using System.Text.Json.Serialization;

namespace Findx.Exceptions;

/// <summary>
///     Findx自定义异常
/// </summary>
public class FindxException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="code"></param>
    [JsonConstructor]
    public FindxException(string code)
    {
        ErrorCode = code;
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    [JsonConstructor]
    public FindxException(string code, string message) : base(message)
    {
        ErrorCode = code;
        ErrorMessage = message;
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    [JsonConstructor]
    public FindxException(string code, string message, Exception exception) : base(message, exception)
    {
        ErrorCode = code;
        //  不拼接内部异常消息：内部异常可能含 SQL 表名/路径/连接串等敏感信息，经全局异常处理器会泄漏给前端；排障信息走日志（InnerException/StackTrace）
        ErrorMessage = message;
    }

    /// <summary>
    ///     错误码
    /// </summary>
    public string ErrorCode { get; private set; }

    /// <summary>
    ///     错误描述
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// 判断是否排除业务异常
    /// </summary>
    /// <param name="isThrow"></param>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <exception cref="FindxException"></exception>
    public static void ThrowIf(bool isThrow, string code, string msg)
    {
        if (isThrow) throw new FindxException(code, msg);
    }
}