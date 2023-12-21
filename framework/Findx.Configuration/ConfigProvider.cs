using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Findx.Configuration
{
    /// <summary>
    ///     Findx配置提供器
    /// </summary>
    internal class ConfigProvider : ConfigurationProvider
    {
        private readonly ConfigClient _client;

        public ConfigProvider(IConfigClient client)
        {
            _client = (ConfigClient)client;
            _client.OnConfigDataChange(x =>
            {
                foreach (var kv in ConfigClient.ConvertChangeDataToJsonConfigDictionary(x)) Data[kv.Key] = kv.Value;
                OnReload();
                return Task.CompletedTask;
            });
            Data = new Dictionary<string, string>();
        }

        [Obsolete("Obsolete")]
        public override void Load()
        {
            _client.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            // Data = _client.Data;
        }
    }
}