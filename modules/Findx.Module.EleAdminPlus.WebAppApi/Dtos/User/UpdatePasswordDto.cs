namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.User;

/// <summary>
///     修改密码参数Dto
/// </summary>
public class UpdatePasswordDto
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; }
}