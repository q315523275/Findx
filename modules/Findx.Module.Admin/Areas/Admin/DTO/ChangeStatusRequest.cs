using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 更改状态入参
    /// </summary>
    public class ChangeStatusRequest
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }

        /// <summary>
        /// 状态-正常_0、停用_1、删除_2
        /// </summary>
        public int Status { get; set; }
    }
}
