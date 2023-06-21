namespace Findx.Setting;

/// <summary>
///     默认配置提供器
/// </summary>
public class DefaultSettingProvider : ISettingProvider
{
    private readonly IEnumerable<ISettingValueProvider> _settingValueProviders;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="settingValueProviders"></param>
    public DefaultSettingProvider(IEnumerable<ISettingValueProvider> settingValueProviders)
    {
        _settingValueProviders = settingValueProviders;
    }

    /// <summary>
    ///     获取key对象
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetObject<T>(string key) where T : new()
    {
        foreach (var item in _settingValueProviders.OrderByDescending(x => x.Order))
            if (item.ContainsKey(key)) return item.GetObject<T>(key);
        return default;
    }

    /// <summary>
    ///     获取值
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValue<T>(string key)
    {
        foreach (var item in _settingValueProviders.OrderByDescending(x => x.Order))
            if (item.ContainsKey(key)) return item.GetValue<T>(key);
        return default;
    }
}