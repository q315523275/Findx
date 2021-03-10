using Findx.Extensions;
using Findx.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace Findx.Configuration
{
    internal class FindxConfigurationProvider : ConfigurationProvider
    {
        private readonly string _LocalBackupPath;
        private readonly FindxConfigurationOptions _options;
        private Task _pollRefreshTask;
        private int _version;
        public FindxConfigurationProvider(FindxConfigurationOptions options)
        {
            _options = options;
            _LocalBackupPath = Path.Combine(Directory.GetCurrentDirectory(), $"local.cache.{options.Namespace}.setting.json");
            Data = new ConcurrentDictionary<string, string>();
        }
        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        private Task PollingRefreshTask()
        {
            if (_options.RefreshInteval == 0)
            {
                return Task.CompletedTask;
            }
            var pollTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_options.RefreshInteval);
                    await LoadAsync();
                }
            });
            return pollTask;
        }
        private async Task LoadAsync(bool reload = false)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = new TimeSpan(0, 0, 30)
                };
                if (reload) { _version = 0; }
                var requestUri = CreateRequestUri(_version);
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                AddOrUpdateData(responseBody);
                File.WriteAllText(_LocalBackupPath, responseBody);
            }
            catch
            {
                if (_version == 0 && File.Exists(_LocalBackupPath))
                {
                    var configText = File.ReadAllText(_LocalBackupPath);
                    AddOrUpdateData(configText);
                }
            }
            finally
            {
                if (_pollRefreshTask == null)
                {
                    _pollRefreshTask = PollingRefreshTask();
                }
            };
        }
        private void AddOrUpdateData(string body)
        {
            var result = body.ToObject<Response>();
            if (result?.Data?.Count > 0)
            {
                foreach (var keyVault in result?.Data)
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
        }
        private string CreateRequestUri(int version = 0)
        {
            var queryPath = $"/configs/{_options.AppId}/{_options.Group}/{_options.Namespace}";
            var queryString = $"version={version}";
            var signString = $"/configs/{_options.AppId}/{_options.AppSercet}/{_options.Group}/{_options.Namespace}/{version}";
            var pathAndQuery = $"{queryPath}?{queryString}&sign=" + Encrypt.SHA256(signString);
            return $"{_options.Address.TrimEnd('/')}{pathAndQuery}";
        }
    }
}
