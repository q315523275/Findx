using System;
using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.Sys.DTO
{
    public class SysUserUpdatePwdRequest
    {
        /// <summary>
        /// 原始密码
        /// </summary>
        [Required(ErrorMessage = "原始密码不能为空")]
        public string Password { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        public string NewPassword { get; set; }

        /// <summary>
        /// 确认密码
        /// </summary>
        [Required(ErrorMessage = "确认密码不能为空"), Compare("NewPassword", ErrorMessage = "两次密码不一致")]
        public string Confirm { get; set; }
    }
}

