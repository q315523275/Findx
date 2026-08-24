using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Findx.Configuration;

/// <summary>
///     配置提供器
/// </summary>
internal class ConfigProvider : ConfigurationProvider
{
    private readonly ConfigClient _client;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="client"></param>
    public ConfigProvider(IConfigClient client)
    {
        _client = (ConfigClient)client;
        _client.OnConfigDataChange(x =>
        {
            //  用于更新默认文件配置
            foreach (var kv in ConfigClient.ConvertToDictionary(x))
            {
                Data[kv.Key] = kv.Value;
            }
            OnReload();
            return Task.CompletedTask;
        });
        Data = new Dictionary<string, string>();
    }
    
    /// <summary>
    ///     加载配置
    /// </summary>
    public override void Load()
    {
        _client.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}