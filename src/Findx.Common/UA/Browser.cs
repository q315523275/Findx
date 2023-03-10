﻿using Findx.Extensions;
using Findx.Utils;

namespace Findx.UA
{
	/// <summary>
	/// 浏览器对象
	/// </summary>
	public class Browser: UserAgentInfo
	{
		/// <summary>
		/// 未知
		/// </summary>
		public static Browser Unknown = new Browser(NameUnknown, null, null);

		/// <summary>
		/// 其它版本
		/// </summary>
		private const string OtherVersion = "[\\/ ]([\\d\\w\\.\\-]+)";

		/// <summary>
		/// 支持的浏览器类型
		/// </summary>
		public static List<Browser> Browers = new List<Browser> {
			new Browser("MSEdge", "Edge|Edg", "(?:edge|Edg)\\/([\\d\\w\\.\\-]+)"),
			new Browser("Chrome", "chrome", "chrome\\/([\\d\\w\\.\\-]+)"),
			new Browser("Firefox", "firefox", OtherVersion),
			new Browser("IEMobile", "iemobile", OtherVersion),
			new Browser("Android Browser", "android", "version\\/([\\d\\w\\.\\-]+)"),
			new Browser("Safari", "safari", "version\\/([\\d\\w\\.\\-]+)"),
			new Browser("Opera", "opera", OtherVersion),
			new Browser("Konqueror", "konqueror", OtherVersion),
			new Browser("PS3", "playstation 3", "([\\d\\w\\.\\-]+)\\)\\s*$"),
			new Browser("PSP", "playstation portable", "([\\d\\w\\.\\-]+)\\)?\\s*$"),
			new Browser("Lotus", "lotus.notes", "Lotus-Notes\\/([\\w.]+)"),
			new Browser("Thunderbird", "thunderbird", OtherVersion),
			new Browser("Netscape", "netscape", OtherVersion),
			new Browser("Seamonkey", "seamonkey", OtherVersion),
			new Browser("Outlook", "microsoft.outlook", OtherVersion),
			new Browser("Evolution", "evolution", OtherVersion),
			new Browser("MSIE", "msie", "msie ([\\d\\w\\.\\-]+)"),
			new Browser("MSIE11", "rv:11", "rv:([\\d\\w\\.\\-]+)"),
			new Browser("Gabble", "Gabble", "Gabble\\/([\\d\\w\\.\\-]+)"),
			new Browser("Yammer Desktop", "AdobeAir", "([\\d\\w\\.\\-]+)\\/Yammer"),
			new Browser("Yammer Mobile", "Yammer[\\s]+([\\d\\w\\.\\-]+)", "Yammer[\\s]+([\\d\\w\\.\\-]+)"),
			new Browser("Apache HTTP Client", "Apache\\\\-HttpClient", "Apache\\-HttpClient\\/([\\d\\w\\.\\-]+)"),
			new Browser("BlackBerry", "BlackBerry", "BlackBerry[\\d]+\\/([\\d\\w\\.\\-]+)"),
			// 企业微信 企业微信使用微信浏览器内核,会包含 MicroMessenger 所以要放在前面
			new Browser("wxwork", "wxwork", "wxwork\\/([\\d\\w\\.\\-]+)"),
			// 微信
			new Browser("MicroMessenger", "MicroMessenger", "MicroMessenger\\/([\\d\\w\\.\\-]+)"),
			// 微信小程序
			new Browser("miniProgram", "miniProgram", "miniProgram\\/([\\d\\w\\.\\-]+)"),
			// 钉钉
			new Browser("DingTalk", "DingTalk", "AliApp\\(DingTalk\\/([\\d\\w\\.\\-]+)\\)")
		};

		/// <summary>
		/// 添加自定义浏览器
		/// </summary>
		/// <param name="name">浏览器名称</param>
		/// <param name="regex">关键字或表达式</param>
		/// <param name="versionRegex">匹配版本的正则</param>
		public static void AddCustomBrowser(string name, string regex, string versionRegex)
		{
			Browers.Add(new Browser(name, regex, versionRegex));
		}

		private readonly string _versionPattern;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name">浏览器名称</param>
		/// <param name="regex">关键字或表达式</param>
		/// <param name="versionRegex">匹配版本的正则</param>
		public Browser(string name, string regex, string versionRegex): base(name, regex)
		{
			if (versionRegex == OtherVersion)
			{
				versionRegex = name + versionRegex;
			}
			if (!versionRegex.IsNullOrWhiteSpace())
			{
				this._versionPattern = versionRegex;
			}
		}

		/// <summary>
		/// 获取浏览器版本
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

		/// <summary>
		/// 是否移动浏览器
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
}

