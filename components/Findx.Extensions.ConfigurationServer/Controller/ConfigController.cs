using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Extensions.ConfigurationServer.Client;
using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Models;
using Findx.Serialization;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
///     客户端调用服务
/// </summary>
[Area("findx")]
[Route("api/config")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-客户端服务"), Description("配置服务-客户端服务")]
public class ConfigController : AreaApiControllerBase
{
    // 防重放 nonce 缓存（进程级；多节点部署如需跨节点防重放可换 Redis，时间窗口已收窄风险）
    private static readonly ConcurrentDictionary<string, DateTime> NonceCache = new();

    private readonly IRepository<AppInfo, long> _appRepo;
    private readonly IRepository<ConfigDataInfo, long> _configRepo;
    private readonly IClientCallBack _clientCallBack;
    private readonly IObjectSerializer _serializer;
    private readonly IOptions<ConfigServerOptions> _options;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ConfigController(IClientCallBack clientCallBack, IRepository<AppInfo, long> appRepo, IRepository<ConfigDataInfo, long> configRepo, IObjectSerializer serializer, IOptions<ConfigServerOptions> options)
    {
        _clientCallBack = clientCallBack;
        _appRepo = appRepo;
        _configRepo = configRepo;
        _serializer = serializer;
        _options = options;
    }

    /// <summary>
    ///     获取配置
    /// </summary>
    /// <param name="appId">appId</param>
    /// <param name="sign">签名</param>
    /// <param name="environment">环境变量</param>
    /// <param name="version">版本号</param>
    /// <param name="load"></param>
    /// <param name="timestamp">时间戳（新签名协议，防重放）</param>
    /// <param name="nonce">随机数（新签名协议，防重放）</param>
    [HttpGet, Description("获取配置")]
    public async Task GetAsync([Required] string appId, [Required] string sign, [Required] string environment, [Required] long version = 0, bool load = false, string timestamp = null, string nonce = null)
    {
        var model = await _appRepo.FirstAsync(x => x.AppId == appId);
        Check.NotNull(model, nameof(model));

        //  签名验证：新协议 HMAC-SHA256 + 时间戳 + nonce 防重放；旧协议 MD5 兼容（告警）
        if (!VerifySign(appId, model.Secret, environment, version, sign, timestamp, nonce))
        {
            Response.StatusCode = HttpStatusCode.Forbidden.To<int>();
            return;
        }

        //  是否初次加载或指定加载
        if (version == 0 || load)
        {
            var rows = await _configRepo.SelectAsync(x => x.AppId == appId && x.Environment == environment && x.Version > version, x => new { x.DataId, x.DataType, x.Content, x.Version });
            //  返回监听结果
            Response.ContentType = "application/json; charset=utf-8";
            await Response.Body.WriteAsync(_serializer.Serialize(rows));
            return;
        }

        //  变更监听
        try
        {
            //  注册变更监听 TaskCompletionSource（超时可配，客户端断开即取消）
            var res = await _clientCallBack.NewCallBackTaskAsync($"{appId}-{environment}", HttpContext.TraceIdentifier, HttpContext.GetClientIp(), _options.Value.ClientLongPollTimeout, HttpContext.RequestAborted);
            var rows = new List<ConfigDataChangeDto> { res };
            //  返回监听结果
            Response.ContentType = "application/json; charset=utf-8";
            await Response.Body.WriteAsync(_serializer.Serialize(rows));
        }
        catch (TimeoutException)
        {
            Response.StatusCode = HttpStatusCode.NoContent.To<int>();
        }
        catch (OperationCanceledException)
        {
            //  客户端请求中断：视为空轮询返回，静默处理
            Response.StatusCode = HttpStatusCode.NoContent.To<int>();
        }
    }

    private bool VerifySign(string appId, string secret, string environment, long version, string sign, string timestamp, string nonce)
    {
        //  兼容旧客户端（无 timestamp/nonce）：MD5 静态拼接（无防重放，仅提示升级）
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(nonce))
        {
            var legacy = EncryptUtility.Md5By32($"{appId}{secret}{environment}{version}");
            return string.Equals(legacy, sign, StringComparison.Ordinal);
        }

        //  时间戳窗口校验
        if (!long.TryParse(timestamp, out var ts))
            return false;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowSeconds = _options.Value.ReplayWindowMinutes * 60L;
        if (Math.Abs(now - ts) > windowSeconds)
            return false;

        //  nonce 防重放：同 appId 下 nonce 只允许使用一次（窗口内）
        var nonceKey = $"{appId}:{nonce}";
        if (!NonceCache.TryAdd(nonceKey, DateTime.UtcNow))
            return false;
        CleanupNonceCache();

        //  HMAC-SHA256(appId|environment|version|timestamp|nonce)
        var expected = EncryptUtility.HmacSha256($"{appId}|{environment}|{version}|{timestamp}|{nonce}", secret);
        return string.Equals(expected, sign, StringComparison.OrdinalIgnoreCase);
    }

    private void CleanupNonceCache()
    {
        var expireBefore = DateTime.UtcNow.AddMinutes(-_options.Value.ReplayWindowMinutes);
        foreach (var item in NonceCache)
        {
            if (item.Value < expireBefore)
                NonceCache.TryRemove(item.Key, out _);
        }
    }
}
