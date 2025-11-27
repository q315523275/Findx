namespace Findx.Module.EleAdminPlus.Shared.Vos.Auth;

/// <summary>
///     用户认证角色简化信息Vo
/// </summary>
public partial class UserAuthRoleSimplifyDto
{
    /// <summary>
    ///     角色id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    ///     角色标识
    /// </summary>
    public string RoleCode { get; set; }
}