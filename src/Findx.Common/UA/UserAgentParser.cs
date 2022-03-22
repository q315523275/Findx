using System;
using Findx.Extensions;
namespace Findx.UA
{
	/// <summary>
	/// User-Agent解析器
	/// </summary>
	public class UserAgentParser
	{
		/// <summary>
		/// 解析User-Agent
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		public static UserAgent Parse(string userAgentString)
		{
			if (userAgentString.IsNullOrWhiteSpace())
			{
				return null;
			}
		    UserAgent userAgent = new UserAgent();

			// 浏览器
			Browser browser = ParseBrowser(userAgentString);
			userAgent.Browser = browser;
			userAgent.BrowserVersion = browser.GetVersion(userAgentString);

			// 浏览器引擎
			Engine engine = ParseEngine(userAgentString);
			userAgent.Engine = engine;
			userAgent.EngineVersion =engine.GetVersion(userAgentString);

			// 操作系统
			OS os = ParseOS(userAgentString);
			userAgent.OS = os;
			userAgent.OsVersion = os.GetVersion(userAgentString);

            // 平台
            Platform platform = ParsePlatform(userAgentString);
            userAgent.Platform = platform;
            userAgent.IsMobile = platform.IsMobile() || browser.IsMobile();

            return userAgent;
		}

		/// <summary>
		/// 解析浏览器类型
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		private static Browser ParseBrowser(String userAgentString)
		{
			foreach (Browser browser in Browser.Browers)
			{
				if (browser.IsMatch(userAgentString))
				{
					return browser;
				}
			}
			return Browser.Unknown;
		}

		/// <summary>
		/// 解析引擎类型
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		private static Engine ParseEngine(string userAgentString)
		{
			foreach (Engine engine in Engine.Engines)
			{
				if (engine.IsMatch(userAgentString))
				{
					return engine;
				}
			}
			return Engine.Unknown;
		}

		/// <summary>
		/// 解析系统类型
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		private static OS ParseOS(string userAgentString)
		{
			foreach (OS os in OS.Oses)
			{
				if (os.IsMatch(userAgentString))
				{
					return os;
				}
			}
			return OS.Unknown;
		}

		/// <summary>
		/// 解析平台类型
		/// </summary>
		/// <param name="userAgentString"></param>
		/// <returns></returns>
		private static Platform ParsePlatform(string userAgentString)
		{
			foreach (Platform platform in Platform.Platforms)
			{
				if (platform.IsMatch(userAgentString))
				{
					return platform;
				}
			}
			return Platform.Unknown;
		}
	}
}

