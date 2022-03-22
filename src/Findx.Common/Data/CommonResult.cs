using System.Text.Json.Serialization;
namespace Findx.Data
{
    public class CommonResult<TData> : CommonResult
    {
        [JsonPropertyName("data")]
        public TData Data { set; get; }
        public CommonResult() { }
        public CommonResult(string code, string msg, TData data) : base(code, msg)
        {
            Data = data;
        }
    }
    public class CommonResult
    {
        public CommonResult(string code, string msg)
        {
            Code = code;
            Msg = msg;
        }
        public CommonResult() { }
        [JsonPropertyName("code")]
        public string Code { set; get; }
        [JsonPropertyName("msg")]
        public string Msg { set; get; }


        public static CommonResult Success()
        {
            return new CommonResult("0000", "操作成功");
        }
        public static CommonResult Success(string msg)
        {
            return new CommonResult("0000", msg);
        }
        public static CommonResult<TData> Success<TData>(TData data)
        {
            return new CommonResult<TData>("0000", "操作成功", data);
        }
        public static CommonResult<TData> Success<TData>(TData data, string msg)
        {
            return new CommonResult<TData>("0000", msg, data);
        }
        public static CommonResult Fail()
        {
            return new CommonResult("500", "失败");
        }
        public static CommonResult<TData> Fail<TData>()
        {
            return new CommonResult<TData>("500", "失败", default);
        }
        public static CommonResult Fail(string code, string msg)
        {
            return new CommonResult(code, msg);
        }
        public static CommonResult<TData> Fail<TData>(string code, string msg)
        {
            return new CommonResult<TData>(code, msg, default);
        }
        public static CommonResult<TData> Fail<TData>(string code, string msg, TData data)
        {
            return new CommonResult<TData>(code, msg, data);
        }
    }
}
