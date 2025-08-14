namespace Findx.Utilities.UA;

/// <summary>
///     User-Agent信息对象
/// </summary>
public class UserAgent
{
	/// <summary>
	///     是否为移动平台
	/// </summary>
	public bool IsMobile { set; get; }

	/// <summary>
	///     浏览器类型
	/// </summary>
	public Browser Browser { set; get; }

	/// <summary>
	///     浏览器版本
	/// </summary>
	public string BrowserVersion { set; get; }

	/// <summary>
	///     平台类型
	/// </summary>
	public Platform Platform { set; get; }

	/// <summary>
	///     系统类型
	/// </summary>
	public Os Os { set; get; }

	/// <summary>
	///     系统版本
	/// </summary>
	public string OsVersion { set; get; }

	/// <summary>
	///     引擎类型
	/// </summary>
	public Engine Engine { set; get; }

	/// <summary>
	///     引擎版本
	/// </summary>
	public string EngineVersion { set; get; }
}