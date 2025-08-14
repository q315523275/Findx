namespace Findx.Utilities.UA;

/// <summary>
///     UserAgent信息
/// </summary>
public class UserAgentInfo
{
	/// <summary>
	///     未知
	/// </summary>
	public static string NameUnknown = "Unknown";

	/// <summary>
	///     Ctor
	/// </summary>
	/// <param name="name"></param>
	/// <param name="regex"></param>
	public UserAgentInfo(string name, string regex)
    {
        Name = name;
        Pattern = regex;
    }

	/// <summary>
	///     信息名称
	/// </summary>
	public string Name { set; get; }

	/// <summary>
	///     信息匹配模式
	/// </summary>
	private string Pattern { get; }

	/// <summary>
	///     指定内容中是否包含匹配此信息的内容
	/// </summary>
	/// <param name="content"></param>
	/// <returns></returns>
	public bool IsMatch(string content)
    {
        return RegexUtility.IsMatch(content, Pattern);
    }

	/// <summary>
	///     是否为Unknown
	/// </summary>
	/// <returns></returns>
	public bool IsUnknown()
    {
        return NameUnknown.Equals(Name);
    }
}