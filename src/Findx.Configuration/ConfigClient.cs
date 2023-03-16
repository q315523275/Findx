using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Utils;

namespace Findx.Configuration
{
    /// <summary>
    /// 配置中心客户端
    /// </summary>
    public class ConfigClient: IConfigClient, IDisposable
    {
        private readonly ConcurrentBag<Func<List<ConfigItemDto>, Task>> _configDataChangeCallbacks;
        private readonly AtomicInteger _nodeIndex;
        private readonly AtomicBoolean _polling;
        private Task _pollingTask;
        private readonly CancellationTokenSource _cts;
        
        public ConfigClient(string appId, string secret, string environment, string servers)
        {
            Check.NotNullOrWhiteSpace(appId, nameof(appId));
            Check.NotNullOrWhiteSpace(secret, nameof(secret));
            Check.NotNullOrWhiteSpace(secret, nameof(secret));
            Check.NotNullOrWhiteSpace(servers, nameof(servers));
            
            AppId = appId;
            AppSecret = secret;
            Environment = environment;
            Servers = servers;

            _nodeIndex = new AtomicInteger(0);
            _polling = new AtomicBoolean(false);
            _configDataChangeCallbacks = new ConcurrentBag<Func<List<ConfigItemDto>, Task>>();
            _cts = new CancellationTokenSource();

            CurrentDataVersion = 0;
            ClientId = Guid.NewGuid().ToString();
        }

        public ConfigClient(ConfigOptions options)
        {
            Check.NotNull(options, nameof(options));
            if (options.Validate().Any())
                throw new ArgumentNullException(nameof(options));

            AppId = options.AppId;
            AppSecret = options.Secret;
            Environment = options.Environment;
            Servers = options.Servers;

            _nodeIndex = new AtomicInteger(0);
            _polling = new AtomicBoolean(false);
            _configDataChangeCallbacks = new ConcurrentBag<Func<List<ConfigItemDto>, Task>>();
            _cts = new CancellationTokenSource();

            CurrentDataVersion = 0;
            ClientId = Guid.NewGuid().ToString();
        }

        #region 属性
        
        /// <summary>
        /// 配置字典
        /// </summary>
        public readonly ConcurrentDictionary<string, string> Data = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// 应用编号
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public string AppSecret { get; }

        /// <summary>
        /// 环境变量
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// 当前数据编号
        /// </summary>
        public long CurrentDataVersion { get; set; }
        
        /// <summary>
        /// 服务节点
        /// </summary>
        public string Servers { get; }
        
        /// <summary>
        /// 当前使用服务
        /// </summary>
        public string CurrentServer { get; set; }
        
        #endregion

        #region 获取配置值
        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key"></param>
        public string this[string key]
        {
            get
            {
                Data.TryGetValue(key, out var val);
                return val;
            }
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            Data.TryGetValue(key, out var val);
            return val;
        }
        #endregion

        #region 获取远端配置信息
        /// <summary>
        /// 生成http地址
        /// </summary>
        /// <returns></returns>
        private string GenerateApiUrl()
        {
            var reqId = Guid.NewGuid().ToString();
            var sign = Encrypt.Md5By32($"{AppId}{AppSecret}{reqId}{Environment}{CurrentDataVersion}");
            return $"{CurrentServer}/api/config?appId={AppId}&sign={sign}&environment={Environment}&reqId={reqId}&version={CurrentDataVersion}";
        }

        /// <summary>
        /// 生成节点Host
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

        public async Task LoadAsync()
        {
            try
            {
                CurrentServer = GenerateServer().TrimEnd('/');
                var apiUrl = GenerateApiUrl();
                using var response = await HttpUtil.GetAsync(apiUrl, null, null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var body = await response.ReadAsStringAsync();
                    var configItem = ConvertToConfigItem(body);
                    if (configItem.Any(x => x.Version > CurrentDataVersion))
                    {
                        // 本地配置更新
                        foreach (var item in configItem)
                        {
                            Data[item.DataId] = item.Content;
                        }
                        // 有配置更新
                        foreach (var callback in _configDataChangeCallbacks)
                        {
                            await callback(configItem);
                        }
                        CurrentDataVersion = configItem.Max(x => x.Version);
                    }
                    
                    // 开启polling
                    if (!_polling.Value && CurrentDataVersion > 0)
                    {
                        _polling.CompareAndSet(false, true);
                        _pollingTask = Task.Factory.StartNew(async ()=>
                        {
                            while (!_cts.IsCancellationRequested)
                            {
                                await LoadAsync();
                            }
                        }, _cts.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_polling.Value)
                {
                    ex.ReThrow();
                }
                else
                {
                    Console.WriteLine($"配置中心循环获取Exception异常:{ex.FormatMessage()}");
                    // 循环存在异常时,进行60秒等待
                    await Task.Delay(60 * 1000);
                }
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 转换配置项
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private static List<ConfigItemDto> ConvertToConfigItem(string body)
        {
            return JsonSerializer.Deserialize<List<ConfigItemDto>>(body, SystemTextJsonStringSerializer.Options);
        }

        /// <summary>
        /// 转换变更数据为字典
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ConvertChangeDataToDictionary(List<ConfigItemDto> list)
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
                        foreach (var kv in JsonConfigurationFileParser.Parse(item.Content))
                        {
                            dict[kv.Key] = kv.Value;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return dict;
        }
        #endregion

        /// <summary>
        /// 添加配置变更回调
        /// </summary>
        /// <param name="callback"></param>
        public void OnConfigDataChange(Func<List<ConfigItemDto>, Task> callback)
        {
            _configDataChangeCallbacks.Add(callback);
        }

        /// <summary>
        /// 是否资源
        /// </summary>
        public void Dispose()
        {
            _pollingTask?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            Data.Clear();
            _configDataChangeCallbacks?.Clear();
        }
    }
}