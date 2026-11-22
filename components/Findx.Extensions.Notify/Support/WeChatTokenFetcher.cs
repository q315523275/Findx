namespace Findx.Extensions.Notify.Support;

/// <summary>
///     微信开放接口 access_token 获取（公众号与小程序共用接口，但账号不同、令牌不通用，各自缓存）
/// </summary>
public static class WeChatTokenFetcher
{
    private const string TokenUrl = "https://api.weixin.qq.com/cgi-bin/token";

    /// <summary>
    ///     拉取令牌
    /// </summary>
    /// <param name="client">HttpClient</param>
    /// <param name="appId">AppId</param>
    /// <param name="appSecret">AppSecret</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>(令牌, 有效期秒)</returns>
    public static async Task<(string Token, int ExpiresIn)> FetchAsync(HttpClient client, string appId, string appSecret, CancellationToken cancellationToken = default)
    {
        var url = $"{TokenUrl}?grant_type=client_credential&appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}";
        var response = await client.GetAsync(url, cancellationToken);
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        var api = text.ToObject<ApiResponse>();
        if (api != null && !string.IsNullOrWhiteSpace(api.Errmsg) && string.IsNullOrWhiteSpace(api.AccessToken))
            throw new InvalidOperationException($"获取微信 access_token 失败：({api.Errcode}) {api.Errmsg}");
        if (api == null || string.IsNullOrWhiteSpace(api.AccessToken))
            throw new InvalidOperationException("获取微信 access_token 失败：响应解析为空");

        return (api.AccessToken, api.ExpiresIn <= 0 ? 7200 : api.ExpiresIn);
    }
}
