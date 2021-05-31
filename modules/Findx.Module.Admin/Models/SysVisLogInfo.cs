using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统访问日志表
    /// </summary>
    public partial class SysVisLog
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 访问账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; } = string.Empty;

        /// <summary>
        /// ip
        /// </summary>
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// 具体消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统
        /// </summary>
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// 是否执行成功（Y-是，N-否）
        /// </summary>
        public string Success { get; set; } = string.Empty;

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime? VisTime { get; set; }

        /// <summary>
        /// 操作类型（字典 1登入 2登出）
        /// </summary>
        public sbyte VisType { get; set; }

    }

}
