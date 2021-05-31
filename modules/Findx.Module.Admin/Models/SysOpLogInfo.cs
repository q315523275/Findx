using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统操作日志表
    /// </summary>
    public partial class SysOpLog
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 操作账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; } = string.Empty;

        /// <summary>
        /// 类名称
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

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
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime? OpTime { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public sbyte? OpType { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Param { get; set; } = string.Empty;

        /// <summary>
        /// 请求方式（GET POST PUT DELETE)
        /// </summary>
        public string ReqMethod { get; set; } = string.Empty;

        /// <summary>
        /// 返回结果
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 是否执行成功（Y-是，N-否）
        /// </summary>
        public string Success { get; set; } = string.Empty;

        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; set; } = string.Empty;

    }

}
