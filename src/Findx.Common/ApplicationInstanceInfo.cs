using Findx.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx
{
    public class ApplicationInstanceInfo : IApplicationInstanceInfo
    {
        private readonly string FindxApplicationRoot = "Findx:Application";
        private readonly Random _random = new Random();
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public ApplicationInstanceInfo(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            ApplicationId = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Id") ?? Guid.NewGuid().ToString();
            ApplicationName = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Name") ?? _environment.ApplicationName;
            Port = _configuration?.GetValue<int>($"{FindxApplicationRoot}:Port") ?? _random.Next(1000, 40000);
            // 启用随机端口
            if (Port == 0)
            {
                Port = _random.Next(1000, 40000);
            }
            Version = _configuration?.GetValue<string>($"{FindxApplicationRoot}:Version") ?? "1.0.1";
            Uris = _configuration?.GetValue<IEnumerable<string>>($"{FindxApplicationRoot}:Uris") ?? new List<string> { $"http://*:{Port}" };
            InstanceIP = DnsUtils.ResolveHostAddress(DnsUtils.ResolveHostName());
        }

        public string ApplicationId { set; get; }

        public string ApplicationName { set; get; }

        public IEnumerable<string> Uris { set; get; }

        public int Port { set; get; }

        public string Version { set; get; }

        public string InstanceIP { set; get; }

        public string InternalIP { set; get; }
    }
}
