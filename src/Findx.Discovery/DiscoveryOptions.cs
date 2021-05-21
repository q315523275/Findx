using Findx.Caching;
using Findx.Utils;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Findx.Discovery
{
    public class DiscoveryOptions : IOptions<DiscoveryOptions>
    {
        private string _hostName;
        private string _scheme = "http";
        public DiscoveryOptions()
        {
            _hostName = DnsUtils.ResolveHostName();
            IpAddress = DnsUtils.ResolveHostAddress(_hostName);
        }
        /// <summary>
        /// 是否启用服务发现客户端
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 服务标签
        /// </summary>
        public IList<string> Tags { get; set; }
        /// <summary>
        /// 是否注册健康检查
        /// </summary>
        public bool RegisterHealthCheck { get; set; } = true;
        /// <summary>
        /// 健康检查地址
        /// </summary>
        public string HealthCheckUrl { get; set; }
        /// <summary>
        /// 健康检查路径
        /// </summary>
        public string HealthCheckPath { get; set; } = "/health";
        /// <summary>
        /// 健康检查轮询时间
        /// </summary>
        public string HealthCheckInterval { get; set; } = "10s";
        /// <summary>
        /// 健康检查超时时间
        /// </summary>
        public string HealthCheckTimeout { get; set; } = "10s";
        /// <summary>
        /// 健康探测失败后,多久进行移除
        /// </summary>
        public string HealthCheckCriticalTimeout { get; set; } = "30m";
        /// <summary>
        /// 健康检查是否跳过Tls认证
        /// </summary>
        public bool HealthCheckTlsSkipVerify { get; set; } = true;
        /// <summary>
        /// 是否注册
        /// </summary>
        public bool Register { get; set; } = true;
        /// <summary>
        /// 是否注销
        /// </summary>
        public bool Deregister { get; set; } = true;
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string HostName
        {
            get => PreferIpAddress ? IpAddress : _hostName;
            set => _hostName = value;
        }
        /// <summary>
        /// 服务注册名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// 服务注册协议
        /// </summary>
        public string Scheme
        {
            get => _scheme;
            set => _scheme = value?.ToLower();
        }
        /// <summary>
        /// 服务注册IP地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 是否使用IP地址进行注册
        /// </summary>
        public bool PreferIpAddress { get; set; } = true;
        /// <summary>
        /// 服务注册端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 服务实例全局唯一编号
        /// </summary>
        public string InstanceId { get; set; }
        /// <summary>
        /// 服务实例组
        /// </summary>
        public string InstanceGroup { get; set; }
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool Cache { get; set; } = false;
        /// <summary>
        /// 缓存策略
        /// </summary>
        public string CacheProvider { get; set; } = CacheType.DefaultMemory;
        /// <summary>
        /// 服务缓存时间,单位秒
        /// </summary>
        public int CacheTTL { get; set; } = 15;
        /// <summary>
        /// OPTIONS
        /// </summary>
        public DiscoveryOptions Value => this;
    }
}
