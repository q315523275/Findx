using Findx.Extensions;
using Findx.Utils;

namespace Findx
{
    /// <summary>
    /// 应用实例信息
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        private const string FindxApplicationRoot = "Findx:Application";
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        /// <param name="hostApplicationLifetime"></param>
        public ApplicationContext(IConfiguration configuration, IHostEnvironment environment, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            ApplicationId = configuration?.GetValue<string>($"{FindxApplicationRoot}:Id") ?? Guid.NewGuid().ToString();
            ApplicationName = configuration?.GetValue<string>($"{FindxApplicationRoot}:Name") ?? environment.ApplicationName;
            Port = configuration?.GetValue<int>($"{FindxApplicationRoot}:Port") ?? GlobalListener.GetAvailablePort(5000);
            if (!GlobalListener.CanListen(Port))
            {
                Port = GlobalListener.GetAvailablePort(5000);
            }
            Version = configuration?.GetValue<string>($"{FindxApplicationRoot}:Version") ?? this.GetType().Assembly.GetProductVersion();
            Uris = configuration?.GetValue<IEnumerable<string>>($"{FindxApplicationRoot}:Uris") ?? new List<string> { $"http://*:{Port}" };
            InstanceIp = DnsUtil.ResolveHostAddress(DnsUtil.ResolveHostName());
            InternalIp = InstanceIp;

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
        public string InstanceIp { get; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string InternalIp { get; }

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
        /// <exception cref="NotImplementedException"></exception>
        public void StopApplication()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
