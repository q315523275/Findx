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

            RootPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public string ApplicationId { set; get; }

        public string ApplicationName { set; get; }

        public IEnumerable<string> Uris { set; get; }

        public int Port { set; get; }

        public string Version { set; get; }

        public string InstanceIP { set; get; }

        public string InternalIP { set; get; }

        public string RootPath { set; get; }

        public string MapPath(string virtualPath) => RootPath + virtualPath.TrimStart('~');
    }
}
