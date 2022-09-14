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
        private readonly string FindxApplicationRoot = "Findx:Application";
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public ApplicationContext(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            ApplicationId = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Id") ?? Guid.NewGuid().ToString();
            ApplicationName = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Name") ?? _environment.ApplicationName;
            Port = _configuration?.GetValue<int>($"{FindxApplicationRoot}:Port") ?? RandomUtil.RandomInt(1000, 40000);
            // 启用随机端口
            if (Port == 0) Port = RandomUtil.RandomInt(1000, 40000);
            Version = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Version") ?? this.GetType().Assembly.GetProductVersion();
            Uris = _configuration?.GetValue<IEnumerable<string>>($"{FindxApplicationRoot}:Uris") ?? new List<string> { $"http://*:{Port}" };
            InstanceIP = DnsUtil.ResolveHostAddress(DnsUtil.ResolveHostName());
            InternalIP = InstanceIP;

            RootPath = environment.ContentRootPath; // AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 应用编号
        /// </summary>
        public string ApplicationId { set; get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationName { set; get; }

        /// <summary>
        /// Uri集合
        /// </summary>
        public IEnumerable<string> Uris { set; get; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { set; get; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { set; get; }

        /// <summary>
        /// 实例Ip
        /// </summary>
        public string InstanceIP { set; get; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string InternalIP { set; get; }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath { set; get; }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public string MapPath(string virtualPath) => RootPath + virtualPath.RemovePreFix("~/");
    }
}
