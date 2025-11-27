namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.User;

/// <summary>
///     用户角色简化信息Vo
/// </summary>
public partial class UserRoleSimplifyVo
{
    /// <summary>
    ///     角色id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     角色标识
    /// </summary>
    public string Code { get; set; }
}