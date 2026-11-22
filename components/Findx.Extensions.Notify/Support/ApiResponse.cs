namespace Findx.Extensions.Notify.Support;

/// <summary>
///     企业微信 / 钉钉 / 微信开放接口的通用返回体（errcode=0 为成功）
/// </summary>
public class ApiResponse
{
    /// <summary>
    ///     错误码。字段缺失时为 null（部分接口成功时只返回数据体）
    /// </summary>
    [JsonPropertyName("errcode")]
    public int? Errcode { get; set; }

    /// <summary>
    ///     错误说明
    /// </summary>
    [JsonPropertyName("errmsg")]
    public string Errmsg { get; set; }

    /// <summary>
    ///     access_token（企业微信 / 微信获取令牌接口）
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    ///     令牌有效期（秒）
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    ///     是否成功
    /// </summary>
    public bool IsOk()
    {
        return Errcode is null or 0;
    }
}
