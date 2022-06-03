using System;
namespace Findx.Module.EleAdmin.DTO
{
	/// <summary>
	/// 设置用户属性值Dto模型
	/// </summary>
	public class SetUserPropertyRequest
	{
		/// <summary>
		/// 编号
		/// </summary>
		public Guid Id { get; set; } = Guid.Empty;

		/// <summary>
		/// 状态
		/// </summary>
		public int Status { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }
	}
}

