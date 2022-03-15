﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx.Utils
{
    /// <summary>
    /// UserAgent操作类
    /// </summary>
    public class UserAgent
    {
        private static readonly IDictionary<string, string> SystemDict = new Dictionary<string, string>()
        {
            { "Windows NT 6.4", "Windows 10" },
            { "Windows NT 6.3", "Windows 8.1 " },
            { "Windows NT 6.2", "Windows 8/Windows Server 2012" },
            { "Windows NT 6.1", "Windows 7/Windows Server 2008 R2" },
            { "Windows NT 6.0", "Windows Vista/Server 2008" },
            { "Windows NT 5.2", "Windows Server 2003" },
            { "Windows NT 5.1", "Windows XP" },
            { "Android", "Android" },
            { "iPhone", "iPhone" },
            { "Mac", "Mac" },
            { "Unix", "UNIX" },
            { "Linux", "Linux" },
            { "SunOS", "SunOS" }
        };
        private static readonly IDictionary<string, string> BrowserDict = new Dictionary<string, string>()
        {
            { "MicroMessenger", "微信内置浏览器" },
            { "MQQBrowser", "手机QQ浏览器" },
            { "SogouMobileBrowser", "搜狗手机浏览器" },
            { "UCBrowser", "UC浏览器" },
            { "UCWEB", "UC浏览器" },
            { "Opera", "Opera浏览器" },
            { "QQBrowser", "QQ浏览器" },
            { "TencentTraveler", "QQ浏览器" },
            { "MetaSr", "搜狗浏览器" },
            { "360SE", "360浏览器" },
            { "MSIE 9.0", "IE 9.0浏览器" },
            { "MSIE 8.0", "IE 8.0浏览器" },
            { "MSIE 7.0", "IE 7.0浏览器" },
            { "MSIE 6.0", "IE 6.0浏览器" },
            { "Firefox", "Firefox浏览器" },
            { "The world", "世界之窗浏览器" },
            { "Maxthon", "遨游浏览器" },
            { "Chrome", "谷歌Chrome浏览器" },
            { "Safari", "Safari内核浏览器" }
        };

        private static string GetSystemString(string agent, string token)
        {
            if (agent.IndexOf(token, StringComparison.OrdinalIgnoreCase) > -1)
            {
                if ("iPhone".Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    return RegexUtil.GetValue(agent, $@"{token}\sOS\s(\d*_)*\d*(?=\s)");
                }
                return RegexUtil.GetValue(agent, $"{token}.*(?=;)");
            }
            return token;
        }

        private readonly string _userAgent;

        /// <summary>
        /// 初始化一个<see cref="UserAgent"/>类型的新实例
        /// </summary>
        public UserAgent(string userAgent)
        {
            _userAgent = userAgent;
        }

        /// <summary>
        /// 获取系统名称
        /// </summary>
        /// <returns></returns>
        public string GetSystem()
        {
            string agent = _userAgent;
            string[] tokens = { "Windows", "Android", "iPhone", "Mac", "Linux" };
            foreach (string token in tokens)
            {
                string system = GetSystemString(agent, token);
                if (SystemDict.ContainsKey(system))
                {
                    return SystemDict[system];
                }
                if (system.Contains(token))
                {
                    return system;
                }
            }
            return SystemDict.FirstOrDefault(m => agent.IndexOf(m.Key, StringComparison.OrdinalIgnoreCase) > -1).Value ?? "未知系统";
        }

        /// <summary>
        /// 获取浏览器
        /// </summary>
        /// <returns></returns>
        public string GetBrowser()
        {
            return BrowserDict.FirstOrDefault(m => _userAgent.IndexOf(m.Key, StringComparison.OrdinalIgnoreCase) > -1).Value ?? "未知浏览器";
        }
    }
}
