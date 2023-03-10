﻿using Findx.Extensions;
using Findx.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Findx.Threading;

namespace Findx.Configuration
{
    /// <summary>
    /// Findx配置提供器
    /// </summary>
    internal class FindxConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly string _localBackupPath;
        private readonly FindxConfigurationOptions _options;
        private FindxAsyncTimer _timer;
        private bool _polling;
        private long _version;
        private HttpClient _httpClient;
        public FindxConfigurationProvider(FindxConfigurationOptions options)
        {
            Data = new Dictionary<string, string>();
            _options = options;
            _localBackupPath = Path.Combine(Directory.GetCurrentDirectory(), $"local.cache.{options.Namespace}.setting.json");
            _httpClient = new HttpClient { Timeout = new TimeSpan(0, 0, 30) };
        }
        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        private async Task PollingRefreshTask(FindxAsyncTimer timer)
        {
            if (_polling) return;

            _polling = true;
            await LoadAsync();
            _polling = false;
        }
        private async Task LoadAsync(bool reload = false)
        {
            try
            {
                if (reload) { _version = 0; }
                var requestUri = CreateRequestUri(_version);
                var response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                AddOrUpdateData(responseBody);
                await File.WriteAllTextAsync(_localBackupPath, responseBody);
            }
            catch
            {
                // 如果是初始化配置报错
                // 从缓存文件中读取最后一次成功数据进行恢复
                if (_version == 0 && File.Exists(_localBackupPath))
                {
                    var configText = await File.ReadAllTextAsync(_localBackupPath);
                    AddOrUpdateData(configText);
                }
            }
            finally
            {
                // 第一次成功后,开启定时获取最新变更配置
                if (_timer == null && _options.RefreshInteval > 0)
                {
                    _timer = new FindxAsyncTimer(null, null)
                    {
                        Elapsed = PollingRefreshTask,
                        Period = _options.RefreshInteval * 1000,
                        RunOnStart = false
                    };
                    _timer.Start();
                }
            }
        }

        private void AddOrUpdateData(string body)
        {
            var result = body.ToObject<ConfigDto>();
            if (!(result?.Data?.Count > 0)) return;
            foreach (var keyVault in result.Data)
            {
                switch (keyVault.VaultType)
                {
                    case VaultType.Text:
                        Data[keyVault.VaultKey] = keyVault.Vault;
                        break;
                    case VaultType.Json:
                    {
                        var jsonKeyVault = JsonConfigurationFileParser.Parse(keyVault.Vault);
                        foreach (var kv in jsonKeyVault)
                        {
                            Data[kv.Key] = kv.Value;
                        }
                    }
                        break;
                }
            }
            _version = result.Version;
            OnReload();
        }
        private string CreateRequestUri(long version = 0)
        {
            var queryPath = $"/configs/{_options.AppId}/{_options.Group}/{_options.Namespace}";
            var queryString = $"version={version}";
            var signString = $"/configs/{_options.AppId}/{_options.AppSercet}/{_options.Group}/{_options.Namespace}/{version}";
            var pathAndQuery = $"{queryPath}?{queryString}&sign=" + Encrypt.Sha256(signString);
            return $"{_options.Endpoint.TrimEnd('/')}{pathAndQuery}";
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer = null;
            _httpClient?.Dispose();
            _httpClient = null;

            GC.SuppressFinalize(this);
        }
    }
}
