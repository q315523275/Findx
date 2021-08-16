using System.Collections.Generic;

namespace Findx
{
    /// <summary>
    /// 应用实例信息
    /// </summary>
    public interface IApplicationInstanceInfo
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
        IEnumerable<string> Uris { get; }

        /// <summary>
        /// 应用端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 应用版本号
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 提供服务IP
        /// </summary>
        string InstanceIP { get; }

        /// <summary>
        /// 真实IP,Docker容器等
        /// </summary>
        string InternalIP { get; }

        /// <summary>
        /// 系统名称
        /// </summary>
        string OSDescription { get; }
    }
}
