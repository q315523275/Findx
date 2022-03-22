using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 通知公告
    /// </summary>
    public class SysNoticeUpdateRequest : SysNoticeCreateRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "通知公告Id不能为空")]
        public long Id { get; set; }
    }
}
