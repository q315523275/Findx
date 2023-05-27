using System.Text.RegularExpressions;

namespace Findx.Utils;

/// <summary>
///     正则操作
/// </summary>
public static class RegexUtil
{
    /// <summary>
    ///     获取匹配值集合
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="pattern">模式字符串</param>
    /// <param name="options">选项</param>
    public static IEnumerable<string> GetValues(string input, string pattern,
        RegexOptions options = RegexOptions.IgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();
        var matches = Regex.Matches(input, pattern, options);
        return from Match match in matches select match.Value;
    }

    /// <summary>
    ///     获取匹配值
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="pattern">模式字符串</param>
    /// <param name="resultPattern">结果模式字符串,范例："$1"用来获取第一个()内的值</param>
    /// <param name="options">选项</param>
    public static string GetValue(string input, string pattern, string resultPattern = "",
        RegexOptions options = RegexOptions.IgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var match = Regex.Match(input, pattern, options);
        if (match.Success == false)
            return string.Empty;
        return string.IsNullOrWhiteSpace(resultPattern) ? match.Value : match.Result(resultPattern);
    }

    /// <summary>
    ///     分割成字符串数组
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="pattern">模式字符串</param>
    /// <param name="options">选项</param>
    public static string[] Split(string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new string[] { };
        return Regex.Split(input, pattern, options);
    }

    /// <summary>
    ///     替换
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="pattern">模式字符串</param>
    /// <param name="replacement">替换字符串</param>
    /// <param name="options">选项</param>
    public static string Replace(string input, string pattern, string replacement,
        RegexOptions options = RegexOptions.IgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        return Regex.Replace(input, pattern, replacement, options);
    }

    /// <summary>
    ///     验证输入与模式是否匹配
    /// </summary>
    /// <param name="input">输入的字符串</param>
    /// <param name="pattern">模式字符串</param>
    /// <param name="options">选项</param>
    public static bool IsMatch(string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
    {
        return Regex.IsMatch(input, pattern, options);
    }
}