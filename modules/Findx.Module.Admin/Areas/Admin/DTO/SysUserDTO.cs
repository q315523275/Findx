using Findx.Data;
using System;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 系统用户DTO模型
    /// </summary>
    public class SysUserDTO : IResponse
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 管理员类型（0超级管理员 1非管理员）
        /// </summary>
        public sbyte AdminType { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 性别(字典 1男 2女 3未知)
        /// </summary>
        public sbyte Sex { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1冻结 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
