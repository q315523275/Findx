using System.Text.RegularExpressions;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行日志脱敏器：默认关闭，由 `Findx:Scheduler:LogMaskingEnabled` 打开。
///     <para>内置规则覆盖常见敏感形态（密码/令牌/Authorization/手机号/身份证号），
///     另可通过 `LogMaskPatterns` 配置自定义正则（命中即替换为 ***）。</para>
///     <para>脱敏发生在"写入采集作用域"这一层：分片上报、明细表、结果回调里的日志摘要都是脱敏后的文本，
///     不存在"某一路漏了原值"的问题。</para>
/// </summary>
public sealed class ExecutionLogMasker
{
    /// <summary>
    ///     内置规则（按顺序应用）
    /// </summary>
    private static readonly Regex[] BuiltInRules =
    [
        //  password=xxx / pwd: xxx
        new(@"(?i)\b(password|passwd|pwd)\s*[:=]\s*[^\s,;&""']+", RegexOptions.Compiled),
        //  token / secret / access_key / apiKey / Authorization: xxx
        new(@"(?i)\b(secret|token|access[_-]?key|secret[_-]?key|api[_-]?key|authorization)\s*[:=]\s*[^\s,;&""']+", RegexOptions.Compiled),
        //  Authorization: Bearer xxx（上面一条已覆盖大多数写法，这条兜底裸 Bearer 串）
        new(@"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]{8,}", RegexOptions.Compiled),
        //  中国大陆手机号
        new(@"(?<!\d)1[3-9]\d{9}(?!\d)", RegexOptions.Compiled),
        //  18 位身份证号
        new(@"(?<![\dXx])\d{17}[\dXx](?![\dXx])", RegexOptions.Compiled)
    ];

    private readonly Regex[] _customRules;

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <param name="customPatterns">自定义正则（可选）</param>
    public ExecutionLogMasker(bool enabled, IEnumerable<string> customPatterns = null)
    {
        Enabled = enabled;

        var rules = new List<Regex>();
        if (enabled && customPatterns != null)
        {
            foreach (var pattern in customPatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern)) continue;
                try
                {
                    rules.Add(new Regex(pattern, RegexOptions.Compiled));
                }
                catch (ArgumentException)
                {
                    //  非法正则：忽略该条，不影响其余规则（配置问题不应让日志采集整体失效）
                }
            }
        }

        _customRules = rules.ToArray();
    }

    /// <summary>
    ///     脱敏一行日志（未启用时原样返回）
    /// </summary>
    public string Mask(string text)
    {
        if (!Enabled || string.IsNullOrEmpty(text)) return text;

        var result = text;

        //  先处理 "Bearer xxx"：否则 Authorization: Bearer xxx 会被键值规则先吃掉 "Bearer" 而漏掉令牌本体
        result = BuiltInRules[2].Replace(result, "Bearer ***");

        //  键值型：保留键名（便于定位是哪个字段），只抹掉值
        result = BuiltInRules[0].Replace(result, m => $"{m.Groups[1].Value}=***");
        result = BuiltInRules[1].Replace(result, m => $"{m.Groups[1].Value}=***");

        //  手机号 / 身份证号：整体替换
        result = BuiltInRules[3].Replace(result, "1**********");
        result = BuiltInRules[4].Replace(result, "******************");

        foreach (var rule in _customRules) result = rule.Replace(result, "***");

        return result;
    }
}
