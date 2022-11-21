using Findx.Extensions;
using Findx.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
namespace Findx
{
    /// <summary>
    /// 应用实例信息
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        private const string FindxApplicationRoot = "Findx:Application";

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public ApplicationContext(IConfiguration configuration, IHostEnvironment environment)
        {
            ApplicationId = configuration?.GetValue<string>($"{FindxApplicationRoot}:Id") ?? Guid.NewGuid().ToString();
            ApplicationName = configuration?.GetValue<string>($"{FindxApplicationRoot}:Name") ?? environment.ApplicationName;
            Port = configuration?.GetValue<int>($"{FindxApplicationRoot}:Port") ?? RandomUtil.RandomInt(1000, 40000);
            // 启用随机端口
            if (Port == 0) Port = RandomUtil.RandomInt(1000, 40000);
            Version = configuration?.GetValue<string>($"{FindxApplicationRoot}:Version") ?? this.GetType().Assembly.GetProductVersion();
            Uris = configuration?.GetValue<IEnumerable<string>>($"{FindxApplicationRoot}:Uris") ?? new List<string> { $"http://*:{Port}" };
            InstanceIP = DnsUtil.ResolveHostAddress(DnsUtil.ResolveHostName());
            InternalIP = InstanceIP;

            RootPath = environment.ContentRootPath; // AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 应用编号
        /// </summary>
        public string ApplicationId { get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// Uri集合
        /// </summary>
        public IEnumerable<string> Uris { get; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 实例Ip
        /// </summary>
        public string InstanceIP { get; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string InternalIP { get; }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public string MapPath(string virtualPath) => RootPath + virtualPath.RemovePreFix("~/");
    }
}
