using Findx.Extensions;
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
    internal class ConfigProvider : ConfigurationProvider
    {
        private readonly ConfigClient _client;

        public ConfigProvider(IConfigClient client)
        {
            _client = (ConfigClient)client;
            _client.OnConfigDataChange(x =>
            {
                foreach (var kv in ConfigClient.ConvertChangeDataToDictionary(x))
                {
                    Data[kv.Key] = kv.Value;
                }
                OnReload();
                return Task.CompletedTask;
            });
            Data = new Dictionary<string, string>();
        }
        
        public override void Load()
        {
            _client.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            // Data = _client.Data;
        }
    }
}
