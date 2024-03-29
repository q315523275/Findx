﻿using Findx.Extensions;

namespace Findx.UA;

/// <summary>
///     User-Agent解析器
/// </summary>
public static class UserAgentParser
{
	/// <summary>
	///     解析User-Agent
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	public static UserAgent Parse(string userAgentString)
    {
        if (userAgentString.IsNullOrWhiteSpace()) return null;
        var userAgent = new UserAgent();

        // 浏览器
        var browser = ParseBrowser(userAgentString);
        userAgent.Browser = browser;
        userAgent.BrowserVersion = browser.GetVersion(userAgentString);

        // 浏览器引擎
        var engine = ParseEngine(userAgentString);
        userAgent.Engine = engine;
        userAgent.EngineVersion = engine.GetVersion(userAgentString);

        // 操作系统
        var os = ParseOs(userAgentString);
        userAgent.OS = os;
        userAgent.OsVersion = os.GetVersion(userAgentString);

        // 平台
        var platform = ParsePlatform(userAgentString);
        userAgent.Platform = platform;
        userAgent.IsMobile = platform.IsMobile() || browser.IsMobile();

        return userAgent;
    }

	/// <summary>
	///     解析浏览器类型
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	private static Browser ParseBrowser(string userAgentString)
    {
        foreach (var browser in Browser.Browers)
            if (browser.IsMatch(userAgentString))
                return browser;
        return Browser.Unknown;
    }

	/// <summary>
	///     解析引擎类型
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	private static Engine ParseEngine(string userAgentString)
    {
        foreach (var engine in Engine.Engines)
            if (engine.IsMatch(userAgentString))
                return engine;
        return Engine.Unknown;
    }

	/// <summary>
	///     解析系统类型
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	private static Os ParseOs(string userAgentString)
    {
        foreach (var os in Os.Oses)
            if (os.IsMatch(userAgentString))
                return os;
        return Os.Unknown;
    }

	/// <summary>
	///     解析平台类型
	/// </summary>
	/// <param name="userAgentString"></param>
	/// <returns></returns>
	private static Platform ParsePlatform(string userAgentString)
    {
        foreach (var platform in Platform.Platforms)
            if (platform.IsMatch(userAgentString))
                return platform;
        return Platform.Unknown;
    }
}