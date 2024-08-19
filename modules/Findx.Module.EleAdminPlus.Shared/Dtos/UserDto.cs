namespace Findx.Module.EleAdminPlus.Shared.Dtos;

/// <summary>
///     用户Dto
/// </summary>
public class UserDto
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
}