namespace Findx
{
    /// <summary>
    /// 应用实例信息
    /// </summary>
    public interface IApplicationContext
    {
        /// <summary>
        /// 应用全局唯一ID
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// 应用绑定URI
        /// </summary>
        string Uris { get; }

        /// <summary>
        /// 应用端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 应用版本号
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 服务IP
        /// </summary>
        string InstanceIp { get; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        string InternalIp { get; }

        /// <summary>
        /// 应用根目录
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// 将虚拟路径转换为物理路径，相对路径转换为绝对路径
        /// </summary>
        /// <param name="virtualPath">虚拟路径</param>
        /// <returns>虚拟路径对应的物理路径</returns>
        string MapPath(string virtualPath);

        /// <summary>
        /// 停止应用
        /// </summary>
        void StopApplication();
    }
}
