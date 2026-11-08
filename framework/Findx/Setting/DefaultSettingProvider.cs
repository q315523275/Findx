namespace Findx.Setting;

/// <summary>
///     默认配置提供器
/// </summary>
public class DefaultSettingProvider : ISettingProvider
{
    private readonly ISettingValueProvider[] _settingValueProviders;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="settingValueProviders"></param>
    public DefaultSettingProvider(IEnumerable<ISettingValueProvider> settingValueProviders)
    {
        //  构造时一次性按 Order 降序物化，避免每次取值重复排序/枚举
        _settingValueProviders = settingValueProviders.OrderByDescending(x => x.Order).ToArray();
    }

    /// <summary>
    ///     获取key对象
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetObject<T>(string key) where T : new()
    {
        foreach (var item in _settingValueProviders)
        {
            if (!item.ContainsKey(key)) continue;
            var result = item.GetObject<T>(key);
            //  值类型装箱后恒非 null，保持“命中即返回”；引用类型为 null 时继续向下游提供器兜底
            if ((object)result != null) return result;
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
        foreach (var item in _settingValueProviders)
        {
            if (!item.ContainsKey(key)) continue;
            var result = item.GetValue<T>(key);
            //  同 GetObject：取值为 null 时不中断链条，继续向下游提供器查询
            if ((object)result != null) return result;
        }
        return default;
    }
}