namespace Findx.Data;

/// <summary>
///     定义实体创建审计信息
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ICreationAudited<TUser> : ICreatedTime where TUser : struct
{
	/// <summary>
	///     创建用户编号
	/// </summary>
	TUser? CreatorId { get; set; }
	
	/// <summary>
	///     创建用户
	/// </summary>
	string Creator { get; set; }
}