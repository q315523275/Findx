using System;
using Findx.Data;

namespace Findx.Admin.Module.System.DTO
{
	public class SetRoleRequest: IRequest
	{
		public int Id { get; set; }

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

