using Findx.Extensions;
using Findx.Utils;

namespace Findx
{
    /// <summary>
    /// 应用实例信息
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly Lazy<string> _version;
        private readonly Lazy<string> _instanceIp;
        private readonly Lazy<string> _internalIp;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="environment"></param>
        /// <param name="hostApplicationLifetime"></param>
        public ApplicationContext(IOptions<ApplicationOptions> options, IHostEnvironment environment, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            
            ApplicationId = options.Value.Id ?? Guid.NewGuid().ToString();
            ApplicationName = options.Value.Name ?? environment.ApplicationName;
            Port = options.Value.Port > 0 ? options.Value.Port : GlobalListener.GetAvailablePort(5000);
            // 验证端口是否被占用
            if (options.Value.AvailablePort && !GlobalListener.CanListen(Port)) 
                Port = GlobalListener.GetAvailablePort(5000);
            Uris = options.Value.Uris ?? $"htt" + $"p://*:{Port}";
            
            _version = new Lazy<string>(() => options.Value.Version ?? this.GetType().Assembly.GetProductVersion());
            _instanceIp = new Lazy<string>(() => options.Value.InstanceIp ?? DnsUtil.ResolveHostAddress(DnsUtil.ResolveHostName()));
            _internalIp = new Lazy<string>(() => options.Value.InternalIp ?? _instanceIp.Value);

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
        public string Uris { get; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version => _version.Value;

        /// <summary>
        /// 实例Ip
        /// </summary>
        public string InstanceIp => _instanceIp.Value;

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string InternalIp => _internalIp.Value;

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

        /// <summary>
        /// 停止应用
        /// </summary>
        public void StopApplication()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
