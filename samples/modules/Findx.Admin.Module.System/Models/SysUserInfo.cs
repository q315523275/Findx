﻿using System;
using System.Collections.Generic;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Admin.Module.System.Models
{
	/// <summary>
    /// 系统用户
    /// </summary>
	[Table(Name = "sys_user")]
	public class SysUserInfo: EntityBaseFullAudited<int, int>, ISoftDeletable, ITenant, IResponse
	{
		/// <summary>
		/// 用户id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override int Id { get; set; }

		/// <summary>
		/// 账号
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 昵称
		/// </summary>
		public string NickName { get; set; }

		/// <summary>
		/// 头像
		/// </summary>
		public string Avatar { get; set; }

		/// <summary>
		/// 性别
		/// </summary>
		public int Sex { get; set; }

		/// <summary>
		/// 手机号
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// 邮箱
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// 邮箱是否验证, 0否, 1是
		/// </summary>
		public int EmailVerified { get; set; }

		/// <summary>
		/// 真实姓名
		/// </summary>
		public string RealName { get; set; }

		/// <summary>
		/// 身份证号
		/// </summary>
		public string IdCard { get; set; }

		/// <summary>
		/// 出生日期
		/// </summary>
		public DateTime? Birthday { get; set; }

		/// <summary>
		/// 个人简介
		/// </summary>
		public string Introduction { get; set; }

		/// <summary>
		/// 机构id
		/// </summary>
		public int OrgId { get; set; }

		/// <summary>
		/// 状态, 0正常, 1冻结
		/// </summary>
		public int Status { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public int? TenantId { get; set; }

		/// <summary>
        /// 是否删除
        /// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// 删除时间
		/// </summary>
		public DateTime? DeletionTime { get; set; }

		/// <summary>
		/// 角色信息
		/// </summary>
		[Navigate(nameof(Id))]
		public virtual List<SysUserRoleInfo> Roles { set; get; }
	}
}
