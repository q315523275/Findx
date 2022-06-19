using System;
using System.Collections.Generic;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	/// <summary>
	/// 设置用户信息Dto模型
	/// </summary>
	public class SetUserRequest: IRequest
	{
		/// <summary>
		/// 编号
		/// </summary>
		public Guid Id { get; set; } = Guid.Empty;

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
		public string Nickname { get; set; }

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
		public Guid? OrgId { get; set; }

		/// <summary>
		/// 角色列表
		/// </summary>
		public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
	}
}

