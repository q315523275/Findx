using System.Text.Json.Serialization;

namespace Findx.Data;

/// <summary>
///     泛型通用结果
/// </summary>
/// <typeparam name="TData"></typeparam>
public class ApiResult<TData> : ApiResult
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public ApiResult() { }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <param name="data"></param>
    public ApiResult(string code, string msg, TData data) : base(code, msg)
    {
        Data = data;
    }

    /// <summary>
    ///     内容
    /// </summary>
    [JsonPropertyName("data")]
    public TData Data { set; get; }
}

/// <summary>
///     通用结果
/// </summary>
public class ApiResult
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    public ApiResult(string code, string msg)
    {
        Code = code;
        Msg = msg;
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public ApiResult() { }

    /// <summary>
    ///     编码
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { set; get; }

    /// <summary>
    ///     描述
    /// </summary>
    [JsonPropertyName("msg")]
    public string Msg { set; get; }

    /// <summary>
    ///     成功
    /// </summary>
    /// <returns></returns>
    public static ApiResult Success()
    {
        return new ApiResult("0000", "操作成功");
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static ApiResult Success(string msg)
    {
        return new ApiResult("0000", msg);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ApiResult<TData> Success<TData>(TData data)
    {
        return new ApiResult<TData>("0000", "操作成功", data);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static ApiResult<TData> Success<TData>(TData data, string msg)
    {
        return new ApiResult<TData>("0000", msg, data);
    }

    /// <summary>
    ///     失败
    ///     <para>默认:code="500"</para>
    /// </summary>
    /// <returns></returns>
    public static ApiResult Fail()
    {
        return new ApiResult("500", "失败");
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public static ApiResult<TData> Fail<TData>()
    {
        return new ApiResult<TData>("500", "失败", default);
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <param name="code">编码</param>
    /// <param name="msg">描述</param>
    /// <returns></returns>
    public static ApiResult Fail(string code, string msg)
    {
        return new ApiResult(code, msg);
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static ApiResult<TData> Fail<TData>(string code, string msg)
    {
        return new ApiResult<TData>(code, msg, default);
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ApiResult<TData> Fail<TData>(string code, string msg, TData data)
    {
        return new ApiResult<TData>(code, msg, data);
    }
}
