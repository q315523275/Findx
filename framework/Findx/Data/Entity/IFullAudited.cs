namespace Findx.Data;

/// <summary>
///     审计全量字段
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface IFullAudited<TUser> : ICreationAudited<TUser>, IUpdateAudited<TUser> where TUser : struct
{
}