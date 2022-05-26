using System.Text.Json.Serialization;
namespace Findx.Data
{
    /// <summary>
    /// 泛型通用结果
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class CommonResult<TData> : CommonResult
    {
        /// <summary>
        /// 内容
        /// </summary>
        [JsonPropertyName("data")]
        public TData Data { set; get; }

        /// <summary>
        /// Ctor
        /// </summary>
        public CommonResult() { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public CommonResult(string code, string msg, TData data) : base(code, msg)
        {
            Data = data;
        }
    }

    /// <summary>
    /// 通用结果
    /// </summary>
    public class CommonResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public CommonResult(string code, string msg)
        {
            Code = code;
            Msg = msg;
        }
        public CommonResult() { }

        /// <summary>
        /// 编码
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { set; get; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonPropertyName("msg")]
        public string Msg { set; get; }

        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static CommonResult Success()
        {
            return new CommonResult("0000", "操作成功");
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static CommonResult Success(string msg)
        {
            return new CommonResult("0000", msg);
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CommonResult<TData> Success<TData>(TData data)
        {
            return new CommonResult<TData>("0000", "操作成功", data);
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static CommonResult<TData> Success<TData>(TData data, string msg)
        {
            return new CommonResult<TData>("0000", msg, data);
        }

        /// <summary>
        /// 失败
        /// <para>默认:code="500"</para>
        /// </summary>
        /// <returns></returns>
        public static CommonResult Fail()
        {
            return new CommonResult("500", "失败");
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        public static CommonResult<TData> Fail<TData>()
        {
            return new CommonResult<TData>("500", "失败", default);
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="msg">描述</param>
        /// <returns></returns>
        public static CommonResult Fail(string code, string msg)
        {
            return new CommonResult(code, msg);
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static CommonResult<TData> Fail<TData>(string code, string msg)
        {
            return new CommonResult<TData>(code, msg, default);
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CommonResult<TData> Fail<TData>(string code, string msg, TData data)
        {
            return new CommonResult<TData>(code, msg, data);
        }
    }

    /// <summary>
    /// 扩展
    /// </summary>
    public static class CommonResultExt
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsSuccess(this CommonResult result)
        {
            return result.Code == "0000";
        }
    }
}
