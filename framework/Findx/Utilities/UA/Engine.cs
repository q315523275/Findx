namespace Findx.Utilities.UA;

/// <summary>
///     引擎对象
/// </summary>
public class Engine : UserAgentInfo
{
	/**
     * 未知
     */
	public static Engine Unknown = new(NameUnknown, null);

    /**
		 * 支持的引擎类型
		 */
    public static List<Engine> Engines =
    [
        new("Trident", "trident"), //
        new("Webkit", "webkit"), //
        new("Chrome", "chrome"), //
        new("Opera", "opera"), //
        new("Presto", "presto"), //
        new("Gecko", "gecko"), //
        new("KHTML", "khtml"), //
        new("Konqeror", "konqueror"), //
        new("MIDP", "MIDP")
    ];

    private readonly string _versionPattern;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="name">引擎名称</param>
    /// <param name="regex">关键字或表达式</param>
    public Engine(string name, string regex) : base(name, regex)
    {
        _versionPattern = name + "[/\\- ]([\\d\\w.\\-]+)";
    }

    /// <summary>
    ///     获取引擎版本
    /// </summary>
    /// <param name="userAgentString"></param>
    /// <returns></returns>
    public string GetVersion(string userAgentString)
    {
        if (IsUnknown()) return null;
        return RegexUtility.GetValue(userAgentString, _versionPattern);
    }
}