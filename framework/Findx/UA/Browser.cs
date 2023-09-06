using Findx.Extensions;
using Findx.Utilities;

namespace Findx.UA;

/// <summary>
///     浏览器对象
/// </summary>
public class Browser : UserAgentInfo
{
	/// <summary>
	///     其它版本
	/// </summary>
	private const string OtherVersion = "[\\/ ]([\\d\\w\\.\\-]+)";

	/// <summary>
	///     未知
	/// </summary>
	public static Browser Unknown = new(NameUnknown, null, null);

	/// <summary>
	///     支持的浏览器类型
	/// </summary>
	public static List<Browser> Browers = new()
    {
        new("MSEdge", "Edge|Edg", "(?:edge|Edg)\\/([\\d\\w\\.\\-]+)"),
        new("Chrome", "chrome", "chrome\\/([\\d\\w\\.\\-]+)"),
        new("Firefox", "firefox", OtherVersion),
        new("IEMobile", "iemobile", OtherVersion),
        new("Android Browser", "android", "version\\/([\\d\\w\\.\\-]+)"),
        new("Safari", "safari", "version\\/([\\d\\w\\.\\-]+)"),
        new("Opera", "opera", OtherVersion),
        new("Konqueror", "konqueror", OtherVersion),
        new("PS3", "playstation 3", "([\\d\\w\\.\\-]+)\\)\\s*$"),
        new("PSP", "playstation portable", "([\\d\\w\\.\\-]+)\\)?\\s*$"),
        new("Lotus", "lotus.notes", "Lotus-Notes\\/([\\w.]+)"),
        new("Thunderbird", "thunderbird", OtherVersion),
        new("Netscape", "netscape", OtherVersion),
        new("Seamonkey", "seamonkey", OtherVersion),
        new("Outlook", "microsoft.outlook", OtherVersion),
        new("Evolution", "evolution", OtherVersion),
        new("MSIE", "msie", "msie ([\\d\\w\\.\\-]+)"),
        new("MSIE11", "rv:11", "rv:([\\d\\w\\.\\-]+)"),
        new("Gabble", "Gabble", "Gabble\\/([\\d\\w\\.\\-]+)"),
        new("Yammer Desktop", "AdobeAir", "([\\d\\w\\.\\-]+)\\/Yammer"),
        new("Yammer Mobile", "Yammer[\\s]+([\\d\\w\\.\\-]+)", "Yammer[\\s]+([\\d\\w\\.\\-]+)"),
        new("Apache HTTP Client", "Apache\\\\-HttpClient", "Apache\\-HttpClient\\/([\\d\\w\\.\\-]+)"),
        new("BlackBerry", "BlackBerry", "BlackBerry[\\d]+\\/([\\d\\w\\.\\-]+)"),
        // 企业微信 企业微信使用微信浏览器内核,会包含 MicroMessenger 所以要放在前面
        new("wxwork", "wxwork", "wxwork\\/([\\d\\w\\.\\-]+)"),
        // 微信
        new("MicroMessenger", "MicroMessenger", "MicroMessenger\\/([\\d\\w\\.\\-]+)"),
        // 微信小程序
        new("miniProgram", "miniProgram", "miniProgram\\/([\\d\\w\\.\\-]+)"),
        // 钉钉
        new("DingTalk", "DingTalk", "AliApp\\(DingTalk\\/([\\d\\w\\.\\-]+)\\)")
    };

    private readonly string _versionPattern;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="name">浏览器名称</param>
    /// <param name="regex">关键字或表达式</param>
    /// <param name="versionRegex">匹配版本的正则</param>
    public Browser(string name, string regex, string versionRegex) : base(name, regex)
    {
        if (versionRegex == OtherVersion) versionRegex = name + versionRegex;
        if (!versionRegex.IsNullOrWhiteSpace()) _versionPattern = versionRegex;
    }

    /// <summary>
    ///     添加自定义浏览器
    /// </summary>
    /// <param name="name">浏览器名称</param>
    /// <param name="regex">关键字或表达式</param>
    /// <param name="versionRegex">匹配版本的正则</param>
    public static void AddCustomBrowser(string name, string regex, string versionRegex)
    {
        Browers.Add(new Browser(name, regex, versionRegex));
    }

    /// <summary>
    ///     获取浏览器版本
    /// </summary>
    /// <param name="userAgentString"></param>
    /// <returns></returns>
    public string GetVersion(string userAgentString)
    {
        if (IsUnknown()) return null;
        return RegexUtility.GetValue(userAgentString, _versionPattern);
    }

    /// <summary>
    ///     是否移动浏览器
    /// </summary>
    /// <returns></returns>
    public bool IsMobile()
    {
        return "PSP".Equals(Name) ||
               "Yammer Mobile".Equals(Name) ||
               "Android Browser".Equals(Name) ||
               "IEMobile".Equals(Name) ||
               "MicroMessenger".Equals(Name) ||
               "miniProgram".Equals(Name) ||
               "DingTalk".Equals(Name);
    }
}