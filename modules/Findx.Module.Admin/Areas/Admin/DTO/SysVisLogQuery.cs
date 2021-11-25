using Findx.Module.Admin.Internals;
using System;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 访问日志查询
    /// </summary>
    public class SysVisLogQuery: Findx.Data.PageBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否执行成功（Y-是，N-否）
        /// </summary>
        public string Success { get; set; }

        /// <summary>
        /// 访问类型
        /// </summary>
        public LoginTypeEnum VisType { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? SearchBeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? SearchEndTime { get; set; }
    }
}
