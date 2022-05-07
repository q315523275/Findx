﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using Findx.Utils;

namespace Findx.UA
{
    /// <summary>
    /// 引擎对象
    /// </summary>
    public class Engine : UserAgentInfo
	{
		/** 未知 */
		public static Engine Unknown = new Engine(NameUnknown, null);

		/**
		 * 支持的引擎类型
		 */
		public static List<Engine> Engines = new List<Engine> { //
				new Engine("Trident", "trident"), //
				new Engine("Webkit", "webkit"), //
				new Engine("Chrome", "chrome"), //
				new Engine("Opera", "opera"), //
				new Engine("Presto", "presto"), //
				new Engine("Gecko", "gecko"), //
				new Engine("KHTML", "khtml"), //
				new Engine("Konqeror", "konqueror"), //
				new Engine("MIDP", "MIDP")//
		};

		private string VersionPattern;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name">引擎名称</param>
		/// <param name="regex">关键字或表达式</param>
		public Engine(string name, string regex) : base(name, regex)
		{
			this.VersionPattern = name + "[/\\- ]([\\d\\w.\\-]+)";
		}

		/// <summary>
		/// 获取引擎版本
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
