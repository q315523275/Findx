using System;
namespace Findx.Data
{
	/// <summary>
	/// 定义实体创建审计信息
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	public interface ICreationAudited<TUser> : ICreatedTime where TUser : struct
	{
		/// <summary>
		/// 更新用户
		/// </summary>
		TUser? CreatorId { get; set; }
	}
}

