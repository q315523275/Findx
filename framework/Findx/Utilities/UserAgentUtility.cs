using Findx.Utilities.UA;

namespace Findx.Utilities;

/// <summary>
///     UserAgent操作类
/// </summary>
public static class UserAgentUtility
{
	/// <summary>
	///     解析User-Agent
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	public static UserAgent Parse(string userAgentString)
    {
        return UserAgentParser.Parse(userAgentString);
    }
}