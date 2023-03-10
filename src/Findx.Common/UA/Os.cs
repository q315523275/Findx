using Findx.Utils;
namespace Findx.UA
{
    /// <summary>
    /// 系统对象
    /// </summary>
    public class Os : UserAgentInfo
	{
		/// <summary>
		/// 未知
		/// </summary>
		public static Os Unknown = new Os(NameUnknown, null);

		/**
		 * 支持的系统类型
		 */
		public static List<Os> Oses = new List<Os> { //
			new Os("Windows 10 or Windows Server 2016", "windows nt 10\\.0", "windows nt (10\\.0)"),//
			new Os("Windows 8.1 or Winsows Server 2012R2", "windows nt 6\\.3", "windows nt (6\\.3)"),//
			new Os("Windows 8 or Winsows Server 2012", "windows nt 6\\.2", "windows nt (6\\.2)"),//
			new Os("Windows Vista", "windows nt 6\\.0", "windows nt (6\\.0)"), //
			new Os("Windows 7 or Windows Server 2008R2", "windows nt 6\\.1", "windows nt (6\\.1)"), //
			new Os("Windows 2003", "windows nt 5\\.2", "windows nt (5\\.2)"), //
			new Os("Windows XP", "windows nt 5\\.1", "windows nt (5\\.1)"), //
			new Os("Windows 2000", "windows nt 5\\.0", "windows nt (5\\.0)"), //
			new Os("Windows Phone", "windows (ce|phone|mobile)( os)?", "windows (?:ce|phone|mobile) (\\d+([._]\\d+)*)"), //
			new Os("Windows", "windows"), //
			new Os("OSX", "os x (\\d+)[._](\\d+)", "os x (\\d+([._]\\d+)*)"), //
			new Os("Android", "Android", "Android (\\d+([._]\\d+)*)"),//
			new Os("Linux", "linux"), //
			new Os("Wii", "wii", "wii libnup/(\\d+([._]\\d+)*)"), //
			new Os("PS3", "playstation 3", "playstation 3; (\\d+([._]\\d+)*)"), //
			new Os("PSP", "playstation portable", "Portable\\); (\\d+([._]\\d+)*)"), //
			new Os("iPad", "\\(iPad.*os (\\d+)[._](\\d+)", "\\(iPad.*os (\\d+([._]\\d+)*)"), //
			new Os("iPhone", "\\(iPhone.*os (\\d+)[._](\\d+)", "\\(iPhone.*os (\\d+([._]\\d+)*)"), //
			new Os("YPod", "iPod touch[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPod touch[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new Os("YPad", "iPad[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPad[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new Os("YPhone", "iPhone[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPhone[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new Os("Symbian", "symbian(os)?"), //
			new Os("Darwin", "Darwin\\/([\\d\\w\\.\\-]+)", "Darwin\\/([\\d\\w\\.\\-]+)"), //
			new Os("Adobe Air", "AdobeAir\\/([\\d\\w\\.\\-]+)", "AdobeAir\\/([\\d\\w\\.\\-]+)"), //
			new Os("Java", "Java[\\s]+([\\d\\w\\.\\-]+)", "Java[\\s]+([\\d\\w\\.\\-]+)")//
		};

		private readonly string _versionPattern;

		/// <summary>
		/// 添加自定义的系统类型
		/// </summary>
		/// <param name="name"></param>
		/// <param name="regex"></param>
		/// <param name="versionRegex"></param>
		public static void AddCustomOs(string name, string regex, string versionRegex)
		{
			Oses.Add(new Os(name, regex, versionRegex));
		}

		/// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regex"></param>
		public Os(string name, string regex): this(name, regex, null)
		{
		}

		/// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regex"></param>
        /// <param name="versionRegex"></param>
		public Os(string name, string regex, string versionRegex): base(name, regex)
		{
			if (!string.IsNullOrWhiteSpace(versionRegex))
			{
				this._versionPattern = versionRegex;
			}
		}

		/// <summary>
		/// 获取系统版本
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		public string GetVersion(string userAgentString)
		{
			if (IsUnknown())
			{
				return null;
			}
			return RegexUtil.GetValue(userAgentString, this._versionPattern);
		}
	}
}

