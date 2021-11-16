using Findx.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 更新用户信息参数
    /// </summary>
    public class SysUserUpdateRequest : IRequest
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号名称不能为空")]
        public string Account { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空")]
        public string NickName { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 性别-男_1、女_2
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
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

        /// <summary>
        /// 员工信息
        /// </summary>
        public SysUserEmpRequest SysEmpParam { get; set; } = new SysUserEmpRequest();
    }
}
