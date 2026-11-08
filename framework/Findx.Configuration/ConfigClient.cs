using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Common.Atomic;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Utilities;
using Microsoft.Extensions.Logging;

namespace Findx.Configuration;

/// <summary>
///     配置中心客户端
/// </summary>
public class ConfigClient : IConfigClient, IDisposable
{
    private readonly ConcurrentBag<Func<List<ConfigItem>, Task>> _changeCallbacks;
    private readonly CancellationTokenSource _cts;
    private readonly AtomicInteger _nodeIndex;
    private readonly AtomicBoolean _polling;
    private Task _pollingTask;
    private bool _requestException = true;
    private ILogger<ConfigClient> _logger;

    /// <summary>
    ///     惰性解析日志（引导期 ServiceLocator 可能尚未就绪，解析失败时返回 null）
    /// </summary>
    private ILogger<ConfigClient> Logger
    {
        get
        {
            if (_logger != null) return _logger;
            
            try
            {
                var factory = ServiceLocator.GetService<ILoggerFactory>();
                _logger = factory?.CreateLogger<ConfigClient>();
            }
            catch
            {
                // 忽略：引导阶段服务提供者未就绪
            }

            return _logger;
        }
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="secret"></param>
    /// <param name="environment"></param>
    /// <param name="servers"></param>
    /// <param name="isRecovery"></param>
    public ConfigClient(string appId, string secret, string environment, string servers, bool isRecovery = false)
    {
        Check.NotNullOrWhiteSpace(appId, nameof(appId));
        Check.NotNullOrWhiteSpace(secret, nameof(secret));
        Check.NotNullOrWhiteSpace(environment, nameof(environment));
        Check.NotNullOrWhiteSpace(servers, nameof(servers));

        AppId = appId;
        AppSecret = secret;
        Environment = environment;
        Servers = servers;
        IsRecovery = isRecovery;

        _nodeIndex = new AtomicInteger(0);
        _polling = new AtomicBoolean(false);
        _changeCallbacks = [];
        _cts = new CancellationTokenSource();

        CurrentDataVersion = 0;
        ClientId = Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    public ConfigClient(ConfigOptions options) : this(
        (options ?? throw new ArgumentNullException(nameof(options))).AppId,
        options.Secret, options.Environment, options.Servers, options.IsRecovery)
    {
        if (options.Validate().Any())
            throw new ArgumentException("配置选项校验未通过", nameof(options));
    }

    #region 属性

    /// <summary>
    ///     配置字典（轮询线程写、业务线程读，使用并发字典）
    /// </summary>
    private readonly ConcurrentDictionary<string, ConfigItem> _data = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     应用编号
    /// </summary>
    public string AppId { get; }

    /// <summary>
    ///     应用密钥
    /// </summary>
    public string AppSecret { get; }

    /// <summary>
    ///     环境变量
    /// </summary>
    public string Environment { get; }

    /// <summary>
    ///     是否异常恢复
    /// </summary>
    public bool IsRecovery { get; }

    /// <summary>
    ///     客户端编号
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    ///     当前数据编号
    /// </summary>
    public long CurrentDataVersion { get; set; }

    /// <summary>
    ///     服务节点
    /// </summary>
    public string Servers { get; }

    /// <summary>
    ///     当前使用服务
    /// </summary>
    public string CurrentServer { get; set; }

    #endregion

    #region 公开方法

    /// <summary>
    ///     获取配置值
    /// </summary>
    /// <param name="key"></param>
    public string this[string key]
    {
        get
        {
            _data.TryGetValue(key, out var val);
            return val?.Content;
        }
    }

    /// <summary>
    ///     获取配置值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Get(string key)
    {
        _data.TryGetValue(key, out var val);
        return val?.Content;
    }

    /// <summary>
    ///     添加配置变更回调
    /// </summary>
    /// <param name="callback"></param>
    public void OnConfigDataChange(Func<List<ConfigItem>, Task> callback)
    {
        _changeCallbacks.Add(callback);
    }

    #endregion

    #region 远程配置

    /// <summary>
    ///     生成http地址
    /// </summary>
    /// <returns></returns>
    private string GenerateApiUrl()
    {
        //  防重放签名协议：HMAC-SHA256(appId|environment|version|timestamp|nonce)，时间戳 + nonce 防重放（服务端已兼容旧 MD5 客户端）
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");
        var sign = EncryptUtility.HmacSha256($"{AppId}|{Environment}|{CurrentDataVersion}|{timestamp}|{nonce}", AppSecret);
        return $"{CurrentServer}/api/config?appId={AppId}&sign={sign}&environment={Environment}&version={CurrentDataVersion}&load={_requestException}&timestamp={timestamp}&nonce={nonce}";
    }

    /// <summary>
    ///     生成节点Host
    /// </summary>
    /// <returns></returns>
    private string GenerateServer()
    {
        var serverList = Servers.Split(";");
        if (!serverList.Any())
            throw new ArgumentNullException(nameof(serverList));

        if (_nodeIndex.Value >= serverList.Length)
            _nodeIndex.Value = 0;

        var index = _nodeIndex.GetAndIncrement();

        return serverList[index];
    }

    /// <summary>
    ///     加载配置（IConfigClient 接口成员，无参转发）
    /// </summary>
    public Task LoadAsync()
    {
        return LoadAsync(CancellationToken.None);
    }

    /// <summary>
    ///     加载配置（带取消令牌）
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            CurrentServer = GenerateServer().TrimEnd('/');
            var apiUrl = GenerateApiUrl();

            using var response = await HttpUtil.GetAsync(apiUrl, null, null, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                //  非 200 视为失败，转入 catch 的容灾/退避路径，避免首次加载静默空配置
                Logger?.LogWarning("配置中心响应非成功状态码: {StatusCode}", (int)response.StatusCode);
                throw new HttpRequestException($"配置中心响应异常，状态码: {(int)response.StatusCode}");
            }

            _requestException = false;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var configItem = ConvertToConfigItems(stream);
            //  配置变更
            if (configItem.Any(x => x.Version > CurrentDataVersion))
            {
                //  更新配置
                await AddOrUpdateConfigAsync(configItem);
                //  保存备份配置
                if (IsRecovery)
                {
                    await SaveObjectFileAsync("appsettings.ConfigCache.json", _data.Values, cancellationToken);
                }
            }

            //  监听更新
            ConfigureChangeListening();
        }
        catch (Exception ex)
        {
            if (!_polling.Value && IsRecovery)
            {
                //  首次请求异常但开启了容灾备份恢复
                var rows = await GetFileObjectAsync<List<ConfigItem>>("appsettings.ConfigCache.json", cancellationToken);
                await AddOrUpdateConfigAsync(rows);
                //  监听更新
                ConfigureChangeListening();
            }
            else if (!_polling.Value)
            {
                //  首次请求异常
                ex.ReThrow();
            }
            else
            {
                //  循环中请求异常
                _requestException = true;

                //  循环存在异常时，进行 30 秒等待（支持取消，避免 Dispose 后长时间滞留）
                try
                {
                    await Task.Delay(30 * 1000, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    //  轮询已停止，不再等待
                }
            }
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     配置变更监听
    /// </summary>
    private void ConfigureChangeListening()
    {
        //  开启polling
        if (!_polling.Value && CurrentDataVersion > 0)
        {
            _polling.CompareAndSet(false, true);
            //  Task.Run 自动解包 async lambda 的内层 Task，_pollingTask 即轮询循环本身，Dispose 可正确等待
            _pollingTask = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await LoadAsync(_cts.Token);
                }
            }, _cts.Token);
        }
    }

    /// <summary>
    ///     更新配置
    /// </summary>
    /// <param name="rows"></param>
    private async Task AddOrUpdateConfigAsync(List<ConfigItem> rows)
    {
        //  本地配置更新
        foreach (var item in rows)
        {
            _data[item.DataId] = item;
        }
        //  有配置更新（单个回调异常不得中断其余回调与轮询主循环）
        foreach (var callback in _changeCallbacks)
        {
            try
            {
                await callback(rows);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "配置变更回调执行异常");
            }
        }
        //  刷新配置版本号
        CurrentDataVersion = rows.Max(x => x.Version);
    }

    /// <summary>
    ///     转换配置项
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private static List<ConfigItem> ConvertToConfigItems(Stream body)
    {
        return JsonSerializer.Deserialize<List<ConfigItem>>(body, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());
    }

    /// <summary>
    ///     根据文件内容返回范型对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task<T> GetFileObjectAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        var file = Path.Combine(System.Environment.CurrentDirectory, path);

        if (!FileUtility.Exists(file)) return default;
        
        try
        {
            var content = await File.ReadAllBytesAsync(file, cancellationToken);
            return JsonSerializer.Deserialize<T>(content.AsSpan(), SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(true));
        }
        catch (Exception)
        {
            throw new Exception($"error trying to get file: {path}");
        }
    }

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task SaveObjectFileAsync(string path, object obj, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));
        Check.NotNull(obj, nameof(obj));

        var content = JsonSerializer.SerializeToUtf8Bytes(obj, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions(true));

        path = path.NormalizePath();
        var file = Path.Combine(System.Environment.CurrentDirectory, path);

        try
        {
            await using var fileStream = CreateFileStream(file);
            await fileStream.WriteAsync(content, 0, content.Length, cancellationToken);
        }
        catch (Exception)
        {
            throw new Exception($"error trying to save file: {path}");
        }
    }

    /// <summary>
    ///     创建真实文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static Stream CreateFileStream(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null) DirectoryUtility.CreateIfNotExists(directory);
        FileUtility.DeleteIfExists(filePath);
        return File.Create(filePath);
    }

    /// <summary>
    ///     转换变更数据为字典 Json内容也变为字典数据
    /// </summary>
    /// <param name="list"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static Dictionary<string, string> ConvertToDictionary(List<ConfigItem> list)
    {
        var dict = new Dictionary<string, string>();
        foreach (var item in list)
        {
            switch (item.DataType)
            {
                case DataType.Text:
                    dict[item.DataId] = item.Content;
                    break;
                case DataType.Json:
                    foreach (var kv in JsonConfigurationFileParser.Parse(item.Content)) dict[kv.Key] = kv.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        return dict;
    }

    #endregion
    
    #region Dispose

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();

        //  等待轮询循环退出（最多 2 秒），避免后台任务在释放后继续访问已清理状态。
        //  正常路径：取消令牌透传 HttpClient.SendAsync，挂起的长轮询请求立即中止，循环毫秒级退出；
        //  超时路径：循环卡在未响应取消的变更回调时记告警，继续强制释放（应用关闭最多被同步阻塞 2 秒）。
        if (_pollingTask != null)
        {
            try
            {
                if (_pollingTask.Wait(TimeSpan.FromSeconds(2)))
                    Logger?.LogDebug("配置客户端轮询循环已退出");
                else
                    Logger?.LogWarning("配置客户端轮询循环未在 2 秒内退出（可能阻塞在未响应取消的变更回调或线程池饥饿），继续强制释放");
            }
            catch (AggregateException ex)
            {
                //  轮询循环以异常结束：不影响释放，仅记录诊断
                Logger?.LogDebug(ex, "配置客户端轮询循环以异常结束");
            }
            catch (Exception)
            {
                //  忽略等待阶段的其他异常
            }
        }

        _pollingTask?.Dispose();
        _cts?.Dispose();

        _data.Clear();
        _changeCallbacks?.Clear();
    }

    #endregion
}