namespace Findx.Extensions.Notify.Support;

/// <summary>
///     access_token 本地缓存（企业微信 / 微信开放接口令牌有效期 7200 秒，需缓存并串行刷新）
/// </summary>
public sealed class AccessTokenProvider
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string _token;
    private DateTime _expiresAt;

    /// <summary>
    ///     取令牌：命中缓存直接返回；未命中则串行调用获取，避免并发刷新把令牌顶掉
    /// </summary>
    /// <param name="fetch">拉取令牌的委托，返回 (令牌, 有效期秒)</param>
    /// <param name="cancellationToken"></param>
    public async Task<string> GetAsync(Func<CancellationToken, Task<(string Token, int ExpiresIn)>> fetch, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_token) && _expiresAt > DateTime.Now) return _token;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            //  双重检查：等锁期间可能已被其它线程刷新
            if (!string.IsNullOrEmpty(_token) && _expiresAt > DateTime.Now) return _token;

            var (token, expiresIn) = await fetch(cancellationToken);
            if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("获取 access_token 失败：接口未返回令牌");

            _token = token;
            //  提前 5 分钟失效，避免边界时刻令牌已过期但本地仍判有效
            _expiresAt = DateTime.Now.AddSeconds(Math.Max(60, expiresIn - 300));
            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    ///     清空缓存（令牌失效或切换账号时可主动调用）
    /// </summary>
    public void Reset()
    {
        _token = null;
        _expiresAt = default;
    }
}
