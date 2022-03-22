using System;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 操作日志查询
    /// </summary>
    public class SysOpLogQuery : Findx.Data.PageBase
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
        /// 请求方式
        /// </summary>
        public string ReqMethod { get; set; }

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
