namespace Findx.Module.EleAdmin.Shared.Dtos;

/// <summary>
///     用户Dto
/// </summary>
public class UserSimplifyDto
{
    /// <summary>
    ///     用户Id
    /// </summary>
    public Guid UserId { set; get; }
    
    /// <summary>
    ///     用户昵称
    /// </summary>
    public string Nickname { set; get; }
    
    /// <summary>
    ///     用户机构Id
    /// </summary>
    public Guid? OrgId { set; get; }
    
    /// <summary>
    ///     用户机构名称
    /// </summary>
    public string OrgName { set; get; }
}