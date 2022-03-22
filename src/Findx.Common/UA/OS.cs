using System.Collections.Generic;
using Findx.Utils;
namespace Findx.UA
{
    /// <summary>
    /// 系统对象
    /// </summary>
    public class OS : UserAgentInfo
	{
		/// <summary>
		/// 未知
		/// </summary>
		public static OS Unknown = new OS(NameUnknown, null);

		/**
		 * 支持的系统类型
		 */
		public static List<OS> Oses = new List<OS> { //
			new OS("Windows 10 or Windows Server 2016", "windows nt 10\\.0", "windows nt (10\\.0)"),//
			new OS("Windows 8.1 or Winsows Server 2012R2", "windows nt 6\\.3", "windows nt (6\\.3)"),//
			new OS("Windows 8 or Winsows Server 2012", "windows nt 6\\.2", "windows nt (6\\.2)"),//
			new OS("Windows Vista", "windows nt 6\\.0", "windows nt (6\\.0)"), //
			new OS("Windows 7 or Windows Server 2008R2", "windows nt 6\\.1", "windows nt (6\\.1)"), //
			new OS("Windows 2003", "windows nt 5\\.2", "windows nt (5\\.2)"), //
			new OS("Windows XP", "windows nt 5\\.1", "windows nt (5\\.1)"), //
			new OS("Windows 2000", "windows nt 5\\.0", "windows nt (5\\.0)"), //
			new OS("Windows Phone", "windows (ce|phone|mobile)( os)?", "windows (?:ce|phone|mobile) (\\d+([._]\\d+)*)"), //
			new OS("Windows", "windows"), //
			new OS("OSX", "os x (\\d+)[._](\\d+)", "os x (\\d+([._]\\d+)*)"), //
			new OS("Android", "Android", "Android (\\d+([._]\\d+)*)"),//
			new OS("Linux", "linux"), //
			new OS("Wii", "wii", "wii libnup/(\\d+([._]\\d+)*)"), //
			new OS("PS3", "playstation 3", "playstation 3; (\\d+([._]\\d+)*)"), //
			new OS("PSP", "playstation portable", "Portable\\); (\\d+([._]\\d+)*)"), //
			new OS("iPad", "\\(iPad.*os (\\d+)[._](\\d+)", "\\(iPad.*os (\\d+([._]\\d+)*)"), //
			new OS("iPhone", "\\(iPhone.*os (\\d+)[._](\\d+)", "\\(iPhone.*os (\\d+([._]\\d+)*)"), //
			new OS("YPod", "iPod touch[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPod touch[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new OS("YPad", "iPad[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPad[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new OS("YPhone", "iPhone[\\s\\;]+iPhone.*os (\\d+)[._](\\d+)", "iPhone[\\s\\;]+iPhone.*os (\\d+([._]\\d+)*)"), //
			new OS("Symbian", "symbian(os)?"), //
			new OS("Darwin", "Darwin\\/([\\d\\w\\.\\-]+)", "Darwin\\/([\\d\\w\\.\\-]+)"), //
			new OS("Adobe Air", "AdobeAir\\/([\\d\\w\\.\\-]+)", "AdobeAir\\/([\\d\\w\\.\\-]+)"), //
			new OS("Java", "Java[\\s]+([\\d\\w\\.\\-]+)", "Java[\\s]+([\\d\\w\\.\\-]+)")//
		};

		private string VersionPattern;

		/// <summary>
		/// 添加自定义的系统类型
		/// </summary>
		/// <param name="name"></param>
		/// <param name="regex"></param>
		/// <param name="versionRegex"></param>
		public static void AddCustomOs(string name, string regex, string versionRegex)
		{
			Oses.Add(new OS(name, regex, versionRegex));
		}

		/// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regex"></param>
		public OS(string name, string regex): this(name, regex, null)
		{
		}

		/// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regex"></param>
        /// <param name="versionRegex"></param>
		public OS(string name, string regex, string versionRegex): base(name, regex)
		{
			if (!string.IsNullOrWhiteSpace(versionRegex))
			{
				this.VersionPattern = versionRegex;
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
			return RegexUtil.GetValue(userAgentString, this.VersionPattern);
		}
	}
}

