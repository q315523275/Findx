using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 更新用户状态入参
    /// </summary>
    public class SysUserStatusUpdateRequest
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
