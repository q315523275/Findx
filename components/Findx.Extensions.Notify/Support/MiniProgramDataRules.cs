namespace Findx.Extensions.Notify.Support;

/// <summary>
///     小程序订阅消息各字段类型的长度限制（微信侧校验不通过会返回 47003，所以在发送前主动收敛）
/// </summary>
public static class MiniProgramDataRules
{
    /// <summary>
    ///     字段类型前缀 → 最大长度。data 的键形如 thing1、character_string12，类型是去掉尾部数字的部分
    /// </summary>
    private static readonly Dictionary<string, int> PrefixLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["thing"] = 20,
        ["phrase"] = 5,
        ["name"] = 10,
        ["symbol"] = 5,
        ["character_string"] = 32,
        ["number"] = 32,
        ["letter"] = 32,
        ["amount"] = 12,
        ["phone_number"] = 17,
        ["car_number"] = 8,
        ["const"] = 20
    };

    /// <summary>
    ///     取字段最大值，未知类型返回 0（不限制）
    /// </summary>
    public static int GetLimit(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return 0;

        var prefix = key.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        return PrefixLimits.TryGetValue(prefix, out var limit) ? limit : 0;
    }

    /// <summary>
    ///     按字段类型截断超长值。时间/日期类不限制长度，原样返回
    /// </summary>
    public static string Truncate(string key, string value, out bool truncated)
    {
        truncated = false;
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;

        var limit = GetLimit(key);
        if (limit <= 0 || value.Length <= limit) return value;

        var cut = limit;
        //  避免在代理对中间截断（emoji 等）
        if (char.IsHighSurrogate(value[cut - 1])) cut--;
        if (cut <= 0) return value;

        truncated = true;
        return value[..cut];
    }
}
