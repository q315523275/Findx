using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 短信查询入参
    /// </summary>
    public class SysSmsQuery : PageBase
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string PhoneNumbers { set; get; }

        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { set; get; }

        /// <summary>
        /// 来源
        /// </summary>
        public int? Source { set; get; }
    }
}
