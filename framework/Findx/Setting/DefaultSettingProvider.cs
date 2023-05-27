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
        {
            var value = item.GetObject<T>(key);
            if (value != null) return value;
        }

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
        {
            var value = item.GetValue<T>(key);
            // 可能多个提供器中存在相同的key,使用EqualityComparer进行判定
            if (value != null && !EqualityComparer<T>.Default.Equals(value, default)) return value;
        }

        return default;
    }
}