﻿using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	/// <summary>
	/// 设置角色信息Dto模型
	/// </summary>
	public class SetRoleRequest: IRequest
	{
		/// <summary>
		/// 编号
		/// </summary>
		public Guid Id { get; set; } = Guid.Empty;

		/// <summary>
		/// 角色名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 角色标识
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Comments { get; set; }
	}
}

