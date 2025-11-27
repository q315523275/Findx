namespace Findx.Module.EleAdminPlus.Shared.Vos.Context;

/// <summary>
///     用户Dto
/// </summary>
public partial class UserContextSimplifyVo
{
    /// <summary>
    ///     用户Id
    /// </summary>
    public long UserId { set; get; }
    
    /// <summary>
    ///     用户昵称
    /// </summary>
    public string Nickname { set; get; }
    
    /// <summary>
    ///     用户机构Id
    /// </summary>
    public long? OrgId { set; get; }
    
    /// <summary>
    ///     用户机构名称
    /// </summary>
    public string OrgName { set; get; }
    
    /// <summary>
    ///     租户Id
    /// </summary>
    public string TenantId { set; get; }
}