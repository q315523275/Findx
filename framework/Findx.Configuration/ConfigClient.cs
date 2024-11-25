using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Utilities;

namespace Findx.Configuration;

/// <summary>
///     配置中心客户端
/// </summary>
public class ConfigClient : IConfigClient, IDisposable
{
    private readonly ConcurrentBag<Func<IEnumerable<ConfigItemDto>, Task>> _changeCallbacks;
    private readonly CancellationTokenSource _cts;
    private readonly AtomicInteger _nodeIndex;
    private readonly AtomicBoolean _polling;
    private Task _pollingTask;
    private bool _requestException = true;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="secret"></param>
    /// <param name="environment"></param>
    /// <param name="servers"></param>
    /// <param name="isRecover"></param>
    public ConfigClient(string appId, string secret, string environment, string servers, bool isRecover = false)
    {
        Check.NotNullOrWhiteSpace(appId, nameof(appId));
        Check.NotNullOrWhiteSpace(secret, nameof(secret));
        Check.NotNullOrWhiteSpace(secret, nameof(secret));
        Check.NotNullOrWhiteSpace(servers, nameof(servers));

        AppId = appId;
        AppSecret = secret;
        Environment = environment;
        Servers = servers;
        IsRecover = isRecover;

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
    public ConfigClient(ConfigOptions options): this(options.AppId, options.Secret, options.Environment, options.Servers, options.IsRecover)
    {
        Check.NotNull(options, nameof(options));
        if (options.Validate().Any())
            throw new ArgumentNullException(nameof(options));
    }

    #region 属性

    /// <summary>
    ///     配置字典
    /// </summary>
    private readonly Dictionary<string, ConfigItemDto> _data = new(StringComparer.OrdinalIgnoreCase);

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
    public bool IsRecover { get; }

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
    public void OnConfigDataChange(Func<IEnumerable<ConfigItemDto>, Task> callback)
    {
        _changeCallbacks.Add(callback);
    }

    #endregion

    #region 获取远端配置信息

    /// <summary>
    ///     生成http地址
    /// </summary>
    /// <returns></returns>
    private string GenerateApiUrl()
    {
        var reqId = Guid.NewGuid().ToString();
        var sign = EncryptUtility.Md5By32($"{AppId}{AppSecret}{reqId}{Environment}{CurrentDataVersion}");
        return
            $"{CurrentServer}/api/config?appId={AppId}&sign={sign}&environment={Environment}&reqId={reqId}&version={CurrentDataVersion}&load={_requestException}";
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
    ///     加载配置
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            CurrentServer = GenerateServer().TrimEnd('/');
            var apiUrl = GenerateApiUrl();

            using var response = await HttpUtil.GetAsync(apiUrl, null, null);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _requestException = false;
                var body = await response.ReadAsStringAsync();
                var configItem = ConvertToConfigItem(body);
                if (configItem.Any(x => x.Version > CurrentDataVersion))
                {
                    // 更新配置
                    await AddOrUpdateConfigAsync(configItem);
                    // 保存备份配置
                    if (IsRecover)
                        await SaveObjectFileAsync("appsettings.ConfigCache.json", _data.Values);
                }

                // 监听更新
                ConfigureUpdateListening();
            }
        }
        catch (Exception ex)
        {
            // 首次请求异常但开启了容灾备份恢复
            if (!_polling.Value && IsRecover)
            {
                // 容灾恢复
                var rows = await GetFileObjectAsync<List<ConfigItemDto>>("appsettings.ConfigCache.json");
                await AddOrUpdateConfigAsync(rows);
                ConfigureUpdateListening();
            }
            // 首次请求异常
            else if (!_polling.Value)
            {
                ex.ReThrow();
            }
            // 循环中请求异常
            else
            {
                _requestException = true;

                // 循环存在异常时,进行60秒等待
                await Task.Delay(30 * 1000);
            }
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     执行循环
    /// </summary>
    private void ConfigureUpdateListening()
    {
        // 开启polling
        if (!_polling.Value && CurrentDataVersion > 0)
        {
            _polling.CompareAndSet(false, true);
            _pollingTask = Task.Factory.StartNew(async () =>
            {
                while (!_cts.IsCancellationRequested) await LoadAsync();
            }, _cts.Token);
        }
    }

    /// <summary>
    ///     更新配置
    /// </summary>
    /// <param name="rows"></param>
    private async Task AddOrUpdateConfigAsync(IEnumerable<ConfigItemDto> rows)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(rows, nameof(rows));
        // 本地配置更新
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var item in rows) _data[item.DataId] = item;
        // 有配置更新
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var callback in _changeCallbacks) await callback(rows);
        // 刷新配置版本号
        // ReSharper disable once PossibleMultipleEnumeration
        CurrentDataVersion = rows.Max(x => x.Version);
    }

    /// <summary>
    ///     转换配置项
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private static List<ConfigItemDto> ConvertToConfigItem(string body)
    {
        return JsonSerializer.Deserialize<List<ConfigItemDto>>(body, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());
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
            return JsonSerializer.Deserialize<T>(content.AsSpan(), SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());
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

        var content = JsonSerializer.SerializeToUtf8Bytes(obj, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());

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
        if (directory != null)
            DirectoryUtility.CreateIfNotExists(directory);
        FileUtility.DeleteIfExists(filePath);
        return File.Create(filePath);
    }

    /// <summary>
    ///     转换变更数据为字典 Json内容也变为字典数据
    /// </summary>
    /// <param name="list"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static IDictionary<string, string> ConvertChangeDataToJsonConfigDictionary(IEnumerable<ConfigItemDto> list)
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
        _pollingTask?.Dispose();
        _cts?.Dispose();

        _data.Clear();
        _changeCallbacks?.Clear();
    }

    #endregion
}