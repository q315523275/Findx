using System;
using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 更新个人资料
    /// </summary>
	public class SysUserUpdateInfoRequest: IRequest
    {
        // 昵称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空")]
        public string NickName { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        [Required(ErrorMessage = "生日不能为空")]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 性别-男_1、女_2
        /// </summary>
        [Required(ErrorMessage = "性别不能为空")]
        public int Sex { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [Required(ErrorMessage = "手机不能为空")]
        public string Phone { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }
    }
}

