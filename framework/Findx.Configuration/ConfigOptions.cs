using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Configuration
{
    /// <summary>
    ///     配置中心客户端配置
    /// </summary>
    public class ConfigOptions : ValidatableObject
    {
        /// <summary>
        ///     配置应用编号
        /// </summary>
        [Required]
        public string AppId { set; get; }

        /// <summary>
        ///     配置应用编号
        /// </summary>
        [Required]
        public string Secret { set; get; }

        /// <summary>
        ///     配置应用编号
        /// </summary>
        [Required]
        public string Environment { set; get; }

        /// <summary>
        ///     配置应用编号
        /// </summary>
        [Required]
        public string Servers { set; get; }

        /// <summary>
        ///     是否进行异常恢复
        /// </summary>
        public bool IsRecover { set; get; }
    }
}